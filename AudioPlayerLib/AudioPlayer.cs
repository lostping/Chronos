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
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioPlayerLib {
  /// <summary>
  /// This class is a replacement for <c>SoundPlayer</c> and <c>MediaPlayer</c> classes. It solves their shortcomings.
  /// For more information, see: http://www.codeproject.com/KB/audio-video/wpfaudioplayer.aspx
  /// </summary>
  public class AudioPlayer : DispatcherObject {
    protected readonly MediaPlayer m_player = new MediaPlayer();

    private bool m_isStopping = false;

    /// <summary>
    /// Indicates whether currently the sound is playing.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// Specifies whether the sound is to be looped. Defaults to <c>false</c>.
    /// </summary>
    public bool IsLooped { get; set; }

    /// <summary>
    /// The volume with which to play the sound. Ranges from 0 to 1 (with 1 being the loudest). Defaults to 1.
    /// </summary>
    private double m_volume;
    public double Volume {
      get { return this.m_volume; }
      set {
        PlayerThread.Instance.Invoke(() => { this.m_player.Volume = value; });
        this.m_volume = value;
      }
    }

    /// <summary>
    /// The length (duration) of this audio file.
    /// </summary>
    private Duration? m_length = null;
    public Duration Length {
      get {
        if (this.m_length == null) {
          PlayerThread.Instance.Invoke(() => { this.m_length = this.m_player.NaturalDuration; });
        }
        return this.m_length.Value; 
      }
    }

    /// <summary>
    /// The position where the audio file is currently playing.
    /// </summary>
    public TimeSpan Position {
      get {
        TimeSpan pos = new TimeSpan();
        PlayerThread.Instance.Invoke(() => { pos = this.m_player.Position; });
        return pos; 
      }
      set {
        PlayerThread.Instance.Invoke(() => { this.m_player.Position = value; });
      }
    }

    /// <summary>
    /// This event is fire if either the playback was stopped by using <see cref="Stop"/> or when a non-looping audio
    /// file has reached its end.
    /// </summary>
    public event EventHandler<EventArgs> PlaybackEnded;

    private DispatcherTimer m_playbackEndedTimer = null;

    /// <summary>
    /// Creates an audio player from a file relative to the executing application's path.
    /// </summary>
    /// <param name="fileName">the audio file to be played</param>
    /// <param name="looping">shall the audio file be played in a loop</param>
    public AudioPlayer(string fileName, bool looping = false) {
      this.m_player = PlayerThread.Instance.CreateMediaPlayer(fileName);
      CompleteInitialization(looping);
    }

    /// <summary>
    /// Creates an audio player from a .NET assembly's (usually DLL) resource file.
    /// </summary>
    /// <param name="assembly">the assembly that contains the audio file. Usually you use 
    /// <c>Assembly.GetExecutingAssembly()</c> to obtain an assembly to be used here.</param>
    /// <param name="assemblyNamespace">the default namespace of the assembly (as specified in the assembly's project
    /// settings)</param>
    /// <param name="mediaFile">the audio file's path relative to the assembly's root. This file's build action must
    /// be set to "Embedded Resource".</param>
    /// <param name="looping">shall the audio file be played in a loop</param>
    public AudioPlayer(Assembly assembly, string assemblyNamespace, string mediaFile, bool looping = false) {
      this.m_player = PlayerThread.Instance.CreateMediaPlayer(assembly, assemblyNamespace, mediaFile);
      CompleteInitialization(looping);
    }

    private void CompleteInitialization(bool looping) {
      this.IsLooped = looping;
      this.IsPlaying = false;

      PlayerThread.Instance.Invoke(() => {
        // NOTE: MediaEnded depends on a Dispatcher running on the player thread. Since Dispatch simply sucks, we don't
        //   have one. Therefor we need to create a timer in "Play()".
        //this.m_player.MediaEnded += OnMediaEnded;
        this.Volume = 1;
      });
    }

    private void OnMediaEnded(object sender, EventArgs e) {
      if (this.m_isStopping) {
        // User explicitly stopped the audio. Don't loop it.
        return;
      }

      this.m_playbackEndedTimer.Stop();

      // IMPORTANT: Don't Stop() the sound here as the timer may be off a little bit.
      if (this.IsLooped) {
        // Reset position to the beginning and play the sound again.
        this.m_isStopping = true;
        PlayerThread.Instance.BeginInvoke(() => {
          this.m_player.Stop();
          this.m_player.Play();
        });
        this.m_playbackEndedTimer.Start();
        this.m_isStopping = false;
      } else {
        OnPlaybackEnded();
      }
    }

    /// <summary>
    /// Starts playing the sound. If <see cref="IsLooped"/> is <c>true</c>, this sound will be played until 
    /// <see cref="Stop"/> is called. Otherwise the sound is played once. Calling this method while the sound is already
    /// playing resets the play position to the beginning of the sound file (ie. the sound is restarted).
    /// </summary>
    public void Play() {
      if (this.m_playbackEndedTimer == null) {
        // Unfortunately "MediaPlayer.MediaEnded" depends on a Dispatcher on the running thread - but we don't have
        // one. Therefor we need to utilize a timer for this.
        this.m_playbackEndedTimer = new DispatcherTimer();
        this.m_playbackEndedTimer.Interval = this.Length.TimeSpan;
        this.m_playbackEndedTimer.Tick += OnMediaEnded;
      }

      this.m_playbackEndedTimer.Stop();
      this.m_isStopping = true;
      PlayerThread.Instance.Invoke(() => {
        this.m_player.Stop();
        this.m_player.Play(); 
      });
      this.m_isStopping = false;
      this.m_playbackEndedTimer.Start();
      this.IsPlaying = true;
    }

    /// <summary>
    /// Stops the current playback. Does nothing, if the sound isn't playing.
    /// </summary>
    public void Stop() {
      if (!this.IsPlaying) {
        return;
      }

      this.m_playbackEndedTimer.Stop();
      this.m_isStopping = true;
      PlayerThread.Instance.Invoke(() => { this.m_player.Stop(); });
      this.m_isStopping = false;
      OnPlaybackEnded();
    }

    private void OnPlaybackEnded() {
      this.IsPlaying = false;
      if (this.PlaybackEnded != null) {
        // Move event handling to the GUI thread
        if (this.Dispatcher.CheckAccess()) {
          this.PlaybackEnded(this, EventArgs.Empty);
        } else {
          this.Dispatcher.BeginInvoke((Action)(() => {
            this.PlaybackEnded(this, EventArgs.Empty);
          }));
        }
      }
    }
  }
}
