//////////////////////////////////////////////////////////////////////////
//
//  Copyright 2011 Sebastian Krysmanski
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
//////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Media;

namespace AudioPlayerLib {
  /// <summary>
  /// This class represents a thread in which all <see cref="MediaPlayer"/> instances are created. This separate thread
  /// is necessary because <c>MediaPlayer.Close()</c> only works from the thread that created the <c>MediaPlayer</c>
  /// instance. Unfortunately, <c>AppDomain.ProcessExit</c> and all destructors (finalizers) run on a separate thread.
  /// Therefor we can't use them to close the <c>MediaPlayer</c> instances - which is required to be able to delete
  /// the temporary files create from resources by this class.
  /// 
  /// <para>Note: This class is just used by <see cref="AudioPlayer"/> and therefor not <c>public</c>.</para>
  /// </summary>
  class PlayerThread {
    private readonly Thread m_playerThread;

    private readonly EventQueue m_threadDispatcher = new EventQueue();
    private readonly string m_resourceTempDir;

    /// <summary>
    /// List of registered players. Players that haven't been closed when the app terminates are closed automatically.
    /// </summary>
    private readonly LinkedList<MediaPlayer> m_registeredPlayers = new LinkedList<MediaPlayer>();

    public static readonly PlayerThread Instance = new PlayerThread();


    private PlayerThread() {
      this.m_resourceTempDir = CreateTempDirectory("resource-extractor-");

      this.m_playerThread = new Thread(RunThread);
      this.m_playerThread.Name = "Allocation Thread";
      this.m_playerThread.IsBackground = true;
      this.m_playerThread.SetApartmentState(ApartmentState.STA);
      this.m_playerThread.Start();

      AppDomain.CurrentDomain.ProcessExit += (s, e) => {
        Shutdown();
      };
    }

    private void Shutdown() {
      // NOTE: When calling this from a shutdown handler ("Closed" or "ProcessExit" events), exceptions thrown in this
      //   method may be ignored (ie. you won't be notified about them). So, this method needs to be tested so that it
      //   doesn't throw any exceptions.

      // NOTE: Unfortunately "Dispatcher.InvokeShutdown" (as well as "BeginInvokeShutdown()") doesn't work here when
      //   this method is called from a "AppDomain.CurrentDomain.ProcessExit" handler - for whatever reason. So, we need
      //   to create our own dispatcher implementation *sigh*. So we need our own event queue implementation.
      this.m_threadDispatcher.InvokeShutdown();

      // Wait for thread to complete.
      this.m_playerThread.Join();

      // Wait to a longer period of time to allow the handles to the files to be freed. Without this, 
      // "Directory.Delete()" may fail and throw an exception.
      Thread.Sleep(200);

      try {
        Directory.Delete(this.m_resourceTempDir, true);
      } catch (IOException) {
        // No op - couldn't delete file or directory.
      }
    }

    public MediaPlayer CreateMediaPlayer(string mediaFile) {
      // NOTE: We need to check whether the file exists because MediaPlayer won't fail loading if the file doesn't 
      //   exist. However, in this case, all of MediaPlayer's properties will be "zero" (eg. Duration).
      if (!File.Exists(Path.GetFullPath(mediaFile))) {
        throw new FileNotFoundException("The audio file '" + mediaFile + "' doesn't exist.");
      }
      Uri uri = new Uri(mediaFile, UriKind.RelativeOrAbsolute);
      return CreateMediaPlayer(uri);
    }

    public MediaPlayer CreateMediaPlayer(Assembly assembly, string assemblyNamespace, string mediaFile) {
      string fullFileName = assemblyNamespace + "." + mediaFile.Replace('/', '.').Replace('\\', '.');
      string tmpFile = Path.Combine(this.m_resourceTempDir, fullFileName);

      // The file may have been created by a previously create media player.
      if (!File.Exists(tmpFile)) {
        using (Stream input = assembly.GetManifestResourceStream(fullFileName)) {
          if (input == null) {
            throw new FileNotFoundException("The audio file '" + fullFileName + "' doesn't exist in the "
                                            + "specified assembly: " + assembly.FullName);
          }
          using (Stream file = File.OpenWrite(tmpFile)) {
            CopyStream(input, file);
          }
        }
      }

      // NOTE: We don't remember the temp file here. It's being deleted along with the whole temp directory when the
      //   app terminates. Note also that we don't delete the file when there are no longer any handles to it to avoid
      //   unnecessary disk i/o when it's being used again at a later time. (It's also hard to determine whether the
      //   file is still in use.)
      return CreateMediaPlayer(new Uri(tmpFile));
    }

    private MediaPlayer CreateMediaPlayer(Uri mediaFile) {
      MediaPlayer player = null;
      Action createAction = () => {
        player = new MediaPlayer();
        player.Open(mediaFile);

        this.m_registeredPlayers.AddLast(player);
      };

      Invoke(createAction);

      return player;
    }

    public void CloseMediaPlayer(MediaPlayer player, bool synchronous = false) {
      Action closeAction = () => {
        player.Close();
        this.m_registeredPlayers.Remove(player);
      };

      if (synchronous) {
        Invoke(closeAction);
      } else {
        BeginInvoke(closeAction);
      }
    }

    public void Invoke(Action action) {
      if (this.m_threadDispatcher.CheckAccess()) {
        action();
      } else {
        this.m_threadDispatcher.Invoke(action);
      }
    }

    public void BeginInvoke(Action action) {
      if (this.m_threadDispatcher.CheckAccess()) {
        action();
      } else {
        this.m_threadDispatcher.BeginInvoke(action);
      }
    }

    private void RunThread() {
      this.m_threadDispatcher.Run();

      foreach (MediaPlayer player in this.m_registeredPlayers) {
        player.Close();
      }
      this.m_registeredPlayers.Clear();
    }

    private static string CreateTempDirectory(string prefix = "") {
      while (true) {
        string folder = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString());
        if (!Directory.Exists(folder)) {
          Directory.CreateDirectory(folder);
          return folder;
        }
      }
    }

    /// <summary>
    /// Copies the contents of input to output. Doesn't close either stream.
    /// </summary>
    private static void CopyStream(Stream input, Stream output) {
      byte[] buffer = new byte[8 * 1024];
      int len;
      while ((len = input.Read(buffer, 0, buffer.Length)) > 0) {
        output.Write(buffer, 0, len);
      }
    }
  }
}
