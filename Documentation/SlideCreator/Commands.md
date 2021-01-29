# Slide Creator Command Reference

Below are the following recognized commands:


- [set](#set)

- [liturgy](#liturgy)
- [break](#break)

- [tlverse](#titled-liturgy-verse)

- [video](#video)
- [fullimage](#full-image)
- [fitimage](#fit-image)
- [autofitimage](#auto-fit-image)
- [litimage](#liturgy-image)



- [reading](#reading)
- [sermon](#sermon)
- [anthemtitle](#anthem-title)
- [2title](#two-line-title)

- [texthymn](#text-hymn)
- [verse](#verse)


- [apostlescreed](#apostles-creed)
- [nicenecreed,](#nicene-creed)
- [lordsprayer](#lords-prayer)
- [copyright](#copyright)
- [viewservices](#view-services)
- [viewseries](#view-series)

- [Resource](#resource)
- [Script](#script)

- [Automation Commands](#automation-commands)

# Set

Sets a project-wide variable.

## Use:

    #set("<somevariable>", "<somevalue>")

## Params:

### somevariable \<string>
- name of the project variable

### somevalue \<string>
- value to set the variable to

Current variables used by project:

## Variable: 'otherspeakers'
Use: 'speakeridentifier-speakertext'

    #set("otherspeakers", "Pastor:-P")

## Variable: 'litspeakertextcol'
Use: 'red,green,blue'

    #set("litspeakertextcol", "255, 0, 0")

## Variable: 'littextcol"
Use: 'red,green,blue'

    #set("littextcol", "255, 0, 0")

## Variable: 'litbackgroundcol'
Use: 'red,green,blue'

    #set("litbackgroundcol", "255, 0, 0")

# Liturgy

Used to apply liturgy layout rules to text.

## Use:

    #liturgy[(<speakerstartonline>)] {

    }

## Params:

### speakerstartonline \<bool> (default=false)
- True: Only detects speakers at the start of a line


Will search source text for *speakers* and then assign lines to them.

## Speakers
The default speakers are:

- P - Pastor
- C - Congregation
- A - Assistant
- L - Leader
- R - Responder
- $ - None

Setting the project variable '[otherspeakers](#variable:-'otherspeakers')' will add user defined speakers d

Accepts an optional parameter to determine if it should only recognize speakers if they start a line (default = false)

    // Default use
    #liturgy {
    P Some example text.
    C This is cool.
    }
    
    // With Parameter
    #liturgy(true) {
    P this should P only detect one speaker.
    }


In the Liturgy source text the character sequence ' T ' will be rendered as a special LSBSymbol character 'T'

## Render Behaviour
All the text within the {} of the #liturgy command will be split onto slides that look like:

    #liturgy {
    P This is some example text.
    C To give you and idea of what to expect.
    }

![image](./img/ExampleLiturgy1.png)


# Break

## Use:
    liturgy...
    #break
    liturgy...

The break command is valid within the #liturgy source text. It will force a slide break.

    #liturgy {
    P Some content that fits on one slide.
    #break
    C This will be forced onto a second slide.
    }


# Titled Liturgy Verse

Renders Liturgy As Centred Text with a title and refence.

## Use:

    #tlverse("<title>", "<reference>", "<drawspeaker>") {
        ...content...
    }

## Params:

### title \<string>
- Slide Title
### reference \<string>
- Slide Reference
### drawspeaker \<string>
- Draw Speaker Labels "true", "false"

## Render Behaviour

Speakers are detected, but not rendered. The renderer will attempt to fit all the text onto the slide, cramming it in if needed.

Example:

    #tlverse("Title", "reference", "false") {
    $ Line 1 of text here.
    $ Second longer line of text here that will be wrapped eventually as it is long enough that it needs 2 lines.
    }

![image](./img/ExampleTLVerse1.png)

Example:

    #tlverse("Title", "reference", "true") {
    P Line 1 here.
    C Line 2 here.
    }

![image](./img/ExampleTLVerse2.png)

# Video

## Use: 
    #video(<assetname>)

## Params:

### assetname \<string>
- Refers to an asset name from the project assets.

This command is automatically used when a video asset is inserted as 'Insert'

## Render Behaviour

The video will be inserted as a slide.


# Full Image

## Use:

    #fullimage(<assetname>)

## Params:

### assetname \<string>
- Refers to an asset name from the project assets.

## Render Behaviour

Renders the image unscaled.

# Fit Image

## Use:

    #fitimage(<assetname>)

### assetname \<string>
- Refers to an asset name from the project assets.

This command is automatically used when a image asset is inserted as 'Insert'

## Render Behaviour

Renders the image with uniform scaling up/down so that the limiting dimension is not clipped.

# Auto Fit Image

## Use:
    #autofitimage(<assetname>)

### assetname \<string>
- Refers to an asset name from the project assets.

This command is automatically used when an asset is inserted as 'Hymn'

## Render Behaviour

Renders the image with uniform scaling up/down so that the limiting dimension is not clipped. Auto detects the 'true' size of the image by inspecting every pixel in the image to find the outer pixel for each direction (top, bottom, left, right) that is not white.
Fills only 93% of slide to pad the image with a white border

# Liturgy Image

## Use:
    #litiamge(<assetname>)

### assetname \<string>
- Refers to an asset name from the project assets.

This command is automatically used when an asset is inserted as 'Liturgy'

## Render Behaviour

Renders the image with uniform scaling up/down so that the limiting dimension is not clipped, based on the 'true size' and then fills 93% available area. Will invert image colors so that black is rendered white and white is rendered black.

# Reading

## Use:

    #reading("<name>", "<reference>")

## Params:

### name \<string>
- name to call the reading. (eg. First Reading, Gospel)

### reference \<string>
- verse reading is from

## Render Behaviour

    #reading("First Reading", "Somewhere 3:5-18")

![image](./img/ExampleReading1.png)


# Sermon

## Use:

    #sermon("<name>", "<reference>", "<preacher>")

## Params:

### name \<string>
- sermon title
### reference \<string>
- text sermon is based upon
### preacher \<string>
- name of the preacher

## Render Behaviour

*The sermon slide will also instruct Integrated Presenter to restart the general purpose timer 1.

    #sermon("'An Insightful Sermon'", "Based Upon Somewhere 2:4-14", "The Rev. Preacher")

![image](./img/ExampleSermon1.png)

# Anthem Title

## Use:

    #anthemtitle("<name>", "<musician>", "<accompanist>", "<credit>")

## Params:

### name \<string>
- name of the anthem
### musician \<string>
- the main performer(s)
### accompanist \<string>
- the accompanists
### credit
- credits/author of piece

## Render Behaviour

    #anthemtitle("Anthem Name", "Main Musician", "Accompanied by (instrument)" "by a Composer")

![image](./img/ExampleAnthemTitle1.png)

# Two Line Title

## Use:

    #2title("<majortext>", "<minortext>", "<orientation>")

## Params:

### majortext \<string>
- the main line of text
### minortext \<string>
- the secondary line of text
### orientation \<string> (default = 'horizontal')
- the layout direction of the lines
- Horizontal layout = both on middle line. Main line left justified (bold). Minor line right justified.
- Vertical layout = two lines. Main line top (centre justified, bold). Minor line bottom (centre justified)

## Render Behaviour

    #2title("THE MAIN LINE", "Secondary Text line", "vertical")

![image](./img/Example2TitleVertical.png)

    #2title("THE MAIN LINE", "Secondary Text line", "horizontal")

![image](./img/Example2TitleHorizontal.png)

# Text Hymn

## Use:

    #texthymn("<title>", "<hymnname>", "<tune>", "<number>", "<copyright>")
    {
        ...#verse{}...
    }

## Params:
### title \<string>
- Slide title
### hymnname \<string>
- Name of the Hymn
### tune \<string>
- Name of Tune (if different)
### number \<string>
- Hymn number and verses
### copyright \<string>
- Copyright info for hymn

Note: Requires at least one [verse](#verse) to render the hymn.
Each verse is rendered on a sperate slide.

# Render Behaviour

    #texthymn("Hymn Title", "Hymn Name", "Alt Tune Name", "Hymnal #111", "Copyright stuff that needs to be put there, but that no-one actually reads")
    {
    #verse {
    Line 1
    Line 2
    Line 3
    Line 4
    } 
    }

![image](./img/ExampleTextHymn1.png)


# Verse

Only valid in a [#texthymn](#text-hymn) command.
Defines the lyrics for a verse.

## Use:

    #verse {
        ...lines...
    }

## Render Behaviour
Each line will be rendered as one line. No attempt will be made to fit lines that are too large/small. Lines will be spaced equidistant vertically.

# Lords Prayer

Prefab slide.

## Use:

    #lordsprayer

## Render Behaviour
Inserts the prebuilt slide for the lords prayer.

![image](./img/ExampleLordsPrayer.png)

# Apostles Creed
Prefab slide.
## Use:
    #apostlescreed
## Render Behaviour
Inserts the 3 prebuilt slide for the lords prayer.

![image](./img/ExampleApostlesCreed1.png)
![image](./img/ExampleApostlesCreed2.png)
![image](./img/ExampleApostlesCreed3.png)

# Nicene Creed
## Use:
    #nicenecreed
Prefab slide.
## Render Behaviour
Inserts the 5 prebuilt slide for the lords prayer.
![image](./img/ExampleNiceneCreed1.png)
![image](./img/ExampleNiceneCreed2.png)
![image](./img/ExampleNiceneCreed3.png)
![image](./img/ExampleNiceneCreed4.png)
![image](./img/ExampleNiceneCreed5.png)

# Copyright
## Use:
    #copyright
Prefab slide.
## Render Behaviour
Inserts the prebuilt slide for the lords prayer.
![image](./img/ExampleCopyright.png)

# View Services
## Use:
    #viewservices
Prefab slide.
## Render Behaviour
Inserts the prebuilt slide for the lords prayer.
![image](./img/ExampleViewServices.png)

# View Series
## Use:
    #viewseries
Prefab slide.
## Render Behaviour
Inserts the prebuilt slide for the lords prayer.
![image](./img/ExampleViewSeries.png)

# Resource

## Use:
    #resource("<assetname>", "<type>")
## Params:
### assetname \<string>
- Asset Name. Name of asset when after importing as asset.
### type \<string>
- Asset Type. "audio", "video", "image". Specifies the export type when including in the slides folder.

Rendered as a file copy. File renamed to Resource_\<assetname>

## Example:

    #resource("Bells1", "audio")

### Rendered as:

Resource_Bells1.wav


# Script

## Use:

    #script {
        // contents here
    }

## Rendered as:

Each script will be rendered as an Action Slide and keyed fully transparent.

# Automation Commands

The scripting language accepts the following commands:

## Arg 0 Commands
- AutoTrans
- CutTrans
- AutoTakePresetIfOnSlide
- DSK1On
- DSK1Off
- DSK1FadeOn
- DSK1FadeOff
- DSK2On
- DSK2Off
- DSK2FadeOn
- DSK2FadeOff
- USK1On
- USK1Off
- USK1SetTypeChroma
- USK1SetTypeDVE
- RecordStart
- RecordStop
- OpenAudioPlayer
- PlayAuxAudio
- StopAuxAudio
- PauseAuxAudio
- ReplayAuxAudio
- PlayMedia
- PauseMedia
- StopMedia
- RestartMedia
- MuteMedia
- UnMuteMedia
- DriveNextSlide
- Timer1Restart

## Arg 1 Commands
- PresetSelect(\<int>) // uses the physical sourceID
- ProgramSelect(\<int>) // uses the physical sourceID
- AuxSelect(\<int>) // uses the physical sourceID
- DelayMs(\<int>) // milliseconds
- LoadAudioFile(\<string>) // exported resource filename

## Syntax

Within the script command body, the following commands syntax is used.

    ('#'<TITLE>) | ((!fullauto) | (['@']'arg0:'<COMMAND_A0>) | (['@']'arg1:'<COMMAND_A1>'('<PARAM>')'))['['<MESSAGE>']'] ';'

Commands are single-line. Commands end with ';'

## Title Command

    #<title>;

Prefixed with '#' text after is marked as the title. (Only visible on the preview of the slide in Integrated Presenter)

## Drive Type Command

    !fullauto;

This marks the slide as fully automated. When the last major action is completed, the automation system will advance to the next slide.

** NOTE **

NO automation will be run on the second slide. i.e. if you're not setup for liturgy and you have a fullauto action slide followed by a liturgy slide, when the fullauto slide's actions are completed it will advance the slide state to the liturgy slide. HOWEVER, the typical automation for liturgy type slides (take preset source if current is slide source then fade on DSK1) WILL NOT occur.

It is expected that since there is an action slide, the action slide will be expected to include the necessary actions for the next slide that would otherwise be handled automatically.

## Statements

There are two types of statements.
1. arg0 statements
2. arg1 statements

arg0 statements take no parameters
arg1 statements take one parameter

Commands can either be executed in setup, or main action mode. Setup actions are run before the presentation transitions to the next (action) slide. Main action slides are executed immediately after the presentation has switched to the action slide.

Setup actions are notated with '@'




## Arg 0 Command

Arg 0 commands take no parameters. They are executed sequentially in the order written.

### Basic Usage
    arg0:<command_name>;
    @arg0:<command_name>;

E.g.

    arg0:DSK1On;

The command is prefixed with the 'arg0:' to identify it has no parameters. The list of recognized arg0 commands is [listed](#arg-0-commands) above.

### Advanced Usage

    arg0:<command_name>[<message>];
    @arg0:<command_name>[<message>];

E.g.

    arg0:DSK1On[Liturgy On];

After the command a message can be included. This will only appear on the preview in Integrated Presenter.

## Arg 1 Command

Arg 1 commands take one parameters. They are executed sequentially in the order written.

### Basic Usage
    arg1:<command_name>(<param>);
    @arg1:<command_name>(<param>);

E.g.

    arg1:PresetSelect(1);

The command is prefixed with the 'arg1:' to identify it has no parameters. The list of recognized arg1 commands is [listed](#arg-1-commands) above.
In round brackets '()' the parameter is provided. It is either a number of string.

### Advanced Usage

    arg1:<command_name>(<param>)[<message>];
    @arg1:<command_name>(<param>)[<message>];

E.g.

    arg0:PresetSelect(1)[Preset Cam 1];

After the command a message can be included. This will only appear on the preview in Integrated Presenter.

