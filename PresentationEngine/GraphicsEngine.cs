using IntegratedPresenter;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.Presentation
{

    public delegate void MediaPositionChanged(MediaPlaybackTimeEventArgs args);

    class GraphicsEngine
    {

        PresenterDisplay _mainDisplay;
        PresenterDisplay _keyDisplay;

        public event MediaPositionChanged OnMainPlayoutPlaybackTimeChanged;

        public void Shutdown()
        {
            _mainDisplay?.Close();
            _keyDisplay?.Close();
        }

        public void Initialize(AutomationEngine automation)
        {
            if (_mainDisplay == null || !_mainDisplay.IsWindowVisilbe)
            {
                _mainDisplay = new PresenterDisplay(automation, false);
                _mainDisplay.Show();
            }
            if (_keyDisplay == null || !_keyDisplay.IsWindowVisilbe)
            {
                _keyDisplay = new PresenterDisplay(automation, true);
                _keyDisplay.Show();
            }

            // Setup Handlers (removing non-existing ones shouldn't matter)
            automation.OnPresentationSlideChanged -= Automation_OnPresentationSlideChanged;
            automation.OnPresentationSlideChanged += Automation_OnPresentationSlideChanged;

            _mainDisplay.OnMediaPlaybackTimeUpdated -= _mainDisplay_OnMediaPlaybackTimeUpdated;
            _mainDisplay.OnMediaPlaybackTimeUpdated += _mainDisplay_OnMediaPlaybackTimeUpdated;
        }

        private void _mainDisplay_OnMediaPlaybackTimeUpdated(object sender, MediaPlaybackTimeEventArgs e)
        {
            OnMainPlayoutPlaybackTimeChanged?.Invoke(e);
        }

        public void PlayMedia()
        {
            _mainDisplay.StartMediaPlayback();
        }

        public void PauseMedia()
        {
            _mainDisplay.PauseMediaPlayback();
        }

        public void StopMedia()
        {
            _mainDisplay.StopMediaPlayback();
        }
        public void RestartMedia()
        {
            _mainDisplay.RestartMediaPlayback();
        }

        public void MuteMedia()
        {
            _mainDisplay.MuteMedia();
        }

        public void UnMuteMedia()
        {
            _mainDisplay.UnMuteMedia();
        }

        private void Automation_OnPresentationSlideChanged(Slide currentslide)
        {
            _mainDisplay?.ShowSlide();
            _keyDisplay?.ShowSlide();
        }
    }
}
