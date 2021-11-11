using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEngine.Playout
{
    public interface ICueableMediaPlayer
    {

        /// <summary>
        /// Submits the media at the specified <paramref name="source"/> <see cref="Uri"/> for cueing by the media player.
        /// <para>See <seealso cref="MaxConcurrentCue"/> for details.</para>
        /// </summary>
        /// <param name="source">The source of the media to cue.</param>
        /// <param name="houseID">An external id that can be supplied to help track cue requests.</param>
        /// <returns>A task that completes when the media is cued.</returns>
        public Task CueMedia(Uri source, Guid houseID);

        /// <summary>
        /// Returns the current set of media that has been requsted for cueing or is already cued.
        /// </summary>
        public IEnumerable<Uri> CuedMedia { get; }

        /// <summary>
        /// Returns the maximum number of concurrent <see cref="CueMedia(Uri)"/> requests that can be processed.
        /// <para>If this is exceeded, the oldest requests will be dropped.</para>
        /// </summary>
        public int MaxConcurrentCue { get; }
        /*
        /// <param name="houseID">The external houseID of the media to put on air. Expected to have already been submitted from a <see cref="CueMedia(Uri, Guid)" request./></param>
        /// <param name="UnCueLasOnAir">If true, marks the currently on-air media for removal from the cue-list once the new media goes on air.</param>
        /// <returns>True if the requested source was succesfully put on air. False if the media was not found to be pre-cued.</returns
        */

        /// <summary>
        /// Attempts to change the current media output to display an already cued media with matching <paramref name="houseID"/>.
        /// </summary>
        /// <param name="houseID">The external houseId of the media to put on air. Expected to have already been submitted from a <see cref="CueMedia(Uri, Guid)"/> request.</param>
        /// <param name="UnCueLastOnAir">If <see langword="true"/>, marks the currently on-air media for removal from the cue-list once the new media goes on air.</param>
        /// <returns><see langword="true"/> if the requested source was succesfully put on-air. <see langword="false"/> if the media was not found to be pre-cued.</returns>
        public bool PutMediaOnAir(Guid houseID, bool UnCueLastOnAir = false);

        /// <summary>
        /// Puts the requested media directly on air without first cueing it. This may take a non-deterministic amount of time and result in unintended playout behaviour.
        /// </summary>
        /// <param name="source">The source of the media to load on air.</param>
        /// <returns>A task the completes when the media is loaded and on air.</returns>
        public Task HotLoadMediOnAir(Uri source);

        



        public enum MediaCueState
        {
            Cued,
            Uncued,
            Cueing,
        }
    }
}
