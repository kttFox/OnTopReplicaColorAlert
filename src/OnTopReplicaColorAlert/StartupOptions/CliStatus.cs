using System;
using System.Collections.Generic;
using System.Text;

namespace OnTopReplicaColorAlert.StartupOptions {
    public enum CliStatus {
        /// <summary>
        /// No errors while parsing.
        /// </summary>
        Ok,
        /// <summary>
        /// User asked for help.
        /// </summary>
        Information,
        /// <summary>
        /// Error while parsing.
        /// </summary>
        Error
    }

}
