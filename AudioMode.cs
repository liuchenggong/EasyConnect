﻿namespace EasyConnect
{
    /// <summary>
    /// Setting to use to forward audio to/from the remote system.
    /// </summary>
    public enum AudioMode
    {
        /// <summary>
        /// Forward the audio from the remote system.
        /// </summary>
        Locally = 0,

        /// <summary>
        /// Forward the audio to the remote system.
        /// </summary>
        Remotely = 1,

        /// <summary>
        /// Do not perform any audio forwarding.
        /// </summary>
        None = 2
    }
}