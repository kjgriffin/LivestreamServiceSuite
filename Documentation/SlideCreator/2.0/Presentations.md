# Presentations

A `Presentation` is simply a folder whose contents conform to a specified naming convention. Presentations are used by *Integrated Presenter*

*Slide Creater* has been developed to produce presentations that conform to the following specifications. However it is theoretically possible to craft one by hand or through other means. The following details the specifications that must be met.

## Presentation Loading

### Slide Types
When loading a presentation, Integrated Presenter will look at every file in the folder. Only the following types will be considered when attempting to turn files into slides. (Other types will be skipped).

- .mp4
- .png
- .txt

Integrated Presenter will attempt to create the appropriate slide from every file it's found in the folder which is one of the aforementioned types.

Files must then match the following convention:

`<slide-number>_<slide-type>.<extension>`

#### Examples:

- 1_Full.png
- 2_Video.mp4
- 3_Liturgy.png

#### Valid slide types:
- `Full` treated as a still image. (Shown using ATEM's Background Source, i.e. as a camera)
- `Liturgy` treated as still graphic (Shown using ATEM's Downstream Keyer)(If the Alpha key is fully masked, then you can achieve the effect of a full frame image, but it's processed through the keyer after the M/E block, not before it)
- `Video` treated as a video.
- `Action` treated as an action slide. See TODO Action Slides for more detail. (is a text [.txt] file that Integrated Presenter will attempt to parse upon presentation loading. Unrecognized commands/invalid scripts will be silently ignored as much as possible)
- `ChromaKeyVideo` (no-longer officially supported)
- `ChromaKeyStill` (no-longer officially  supported)

#### Special Cases:

Slides can optionally have an action in the filename

	<slide-number>_<slide-type>-<action>.<extension>

The only action that is currently supported is
- t1restart (Restarts timer 1)

Note: This is currently only used on the 'Sermon' slide. It's not recommend to make heavy use of this feature.

Slides can also be marked to ignore automation. e.g.
- 1_Liturgy.nodrive.png
- 2_Full.nodrive.png

Performing the 'Next Slide' in Integrated Presenter when going to a .nodrive slide will have the same effect as manually taking Integrated Presenter out of drive mode and then going to the next slide. (i.e. only the generated graphics that are input to the switcher will change. No commands will be run that manipulate the switcher)

### Key files:

In addition to 'slides' the presentation will attempt to load key files for each slide.
For every recognized slide that was loaded, loading will also check for an optional key file to associate with the slide.
Key files follow the following naming convention:
`Key_<slide-number>`

Key files are expected to be [.png] files, unless they're [.mp4] (See TODO Video keys for info about that)

If no key file was supplied, a default will be used. A full frame black key will be used for every slide type, except in the case of a Full slide, in which case a full frame white key will be used.
In the case that the slide itself has no media (i.e. action files) the 'slides' media file will use a full frame black image. (Except when the provided script specifies an override: SEE TODO FOR DETAILS)




### Additionally Integrated Presenter will check if the following optional files exist:

- BMDSwitcherConfig.json

The optional BMDSwitcherConfig.json specifies the config to apply on the ATEM switcher. It will be applied immediately upon loading the presentation.
In the case there is no active switcher connection (mock or hardware) the config will be stored internally, and applied immediately upon connection.

If this is not provided, IntegratedPresenter will use its internal default config. This will be applied at the first connection to the a switcher target. It will not be re-applied when loading a presentation. (i.e., if you've made changes via some other method like ATEM Software Control, those will persist).

- IntegratedPresenterUserConfig.json

The optional IntegratedPresenterUserConfig.json file provides options to configure features/settings mostly pertaining to the UI/Features of Integrated Presenter. It will be applied immediately upon loading the presentation. If no config was provided nothing will change.