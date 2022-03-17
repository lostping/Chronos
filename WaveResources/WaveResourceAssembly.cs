using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WaveResources {
  public static class WaveResourceAssembly {
    public static Assembly Assembly {
      get { return Assembly.GetExecutingAssembly(); }
    }
  }
}
