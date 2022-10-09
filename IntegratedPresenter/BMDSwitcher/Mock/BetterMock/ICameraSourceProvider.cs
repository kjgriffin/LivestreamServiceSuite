using System.Windows.Media.Imaging;

namespace IntegratedPresenter.BMDSwitcher.Mock
{
    public interface ICameraSourceProvider
    {
        public bool TryGetSourceImage(int PhysicalInputID, out BitmapImage image);
        bool TryGetSourceVideo(int PhysicalInputID, out string source);
        internal bool TryGetLiveCamState(int PhysicalInputID, out LiveCameraState camState);
    }
}
