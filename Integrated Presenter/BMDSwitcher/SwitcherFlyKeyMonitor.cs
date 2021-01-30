using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    public delegate void KeyFrameEventHandler(object sender, int keyframe);
    class SwitcherFlyKeyMonitor: IBMDSwitcherKeyFlyParametersCallback
    {

        public event KeyFrameEventHandler KeyFrameChanged;
        public event SwitcherEventHandler KeyFrameStateChange;

        public void Notify(_BMDSwitcherKeyFlyParametersEventType eventType, _BMDSwitcherFlyKeyFrame keyFrame)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeFlyChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeCanFlyChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRateChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeXChanged:
                    KeyFrameStateChange?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeSizeYChanged:
                    KeyFrameStateChange?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionXChanged:
                    KeyFrameStateChange?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypePositionYChanged:
                    KeyFrameStateChange?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeRotationChanged:
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsKeyFrameStoredChanged:
                    KeyFrameStateChange?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsAtKeyFramesChanged:
                    int frame = -1;
                    if (keyFrame == _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameA)
                        frame = 1;
                    if (keyFrame == _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameB)
                        frame = 2;
                    if (keyFrame == _BMDSwitcherFlyKeyFrame.bmdSwitcherFlyKeyFrameFull)
                        frame = 0;
                    KeyFrameChanged?.Invoke(this, frame);
                    break;
                case _BMDSwitcherKeyFlyParametersEventType.bmdSwitcherKeyFlyParametersEventTypeIsRunningChanged:
                    break;
                default:
                    break;
            }
        }
    }
}
