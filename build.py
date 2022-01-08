import json
import os
import argparse

def main():

    parser = argparse.ArgumentParser(description="Generate Official Builds")
    parser.add_argument("mode", type=str, default="Release", help="build mode: Experimental/Release etc.")

    args = parser.parse_args()
    print(args.mode)

    print("starting builds...")
    with open('./SlideCreater/version.json') as f:
        version = json.load(f)
    # get build numbers
    majorversion = version['MajorVersion']
    minorversion = version['MinorVersion']
    revison = version['Revision']
    build = version['Build']
    

    # change to release build
    version['Mode'] = args.mode
    with open('./SlideCreater/version.json', "w") as f:
        json.dump(version, f)

    print(version)

    # create build number string
    bnum = str(build).zfill(3)
    bnumstr = "{0}.{1}.{2}.{3}-{4}".format(majorversion, minorversion, revison, bnum, args.mode)
    print("build version: {0}".format(bnumstr))

    # build SlideCreater
    #buildcmd_slidecreater = 'dotnet publish SlideCreater -c Release -p:PublishDir=.\..\publish\{0}\SlideCreater -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -r win-x64-aot'.format(bnumstr)
    buildcmd_slidecreater = 'msbuild "SlideCreater" -r /t:publish /p:PublishDir=".\..\publish\{0}\SlideCreater" /p:Configuration=Release /p:SelfContained=true /p:RuntimeIdentifier=win-x64-aot /p:PublishSingleFile=true'.format(bnumstr)
    os.system(buildcmd_slidecreater) 
    # rename with build number
    cwd = os.getcwd()
    scpath = os.path.join(cwd, "publish", bnumstr, "SlideCreater")
    print(scpath)
    os.rename("{0}\SlideCreater.exe".format(scpath), "{0}\SlideCreater_{1}_win_x64.exe".format(scpath, bnumstr))

    # increment build number
    build += 1
    version["Build"] = build
    # save new build number
    version['Mode'] = "Debug"
    with open('./SlideCreater/version.json', "w") as f:
        json.dump(version, f)


    with open('./IntegratedPresenter/version.json') as f:
        version = json.load(f)
    # get build numbers
    majorversion = version['MajorVersion']
    minorversion = version['MinorVersion']
    revison = version['Revision']
    build = version['Build']

    # change to release build
    version['Mode'] = args.mode
    with open('./IntegratedPresenter/version.json', "w") as f:
        json.dump(version, f)

    print(version)


    # create build number string
    bnum = str(build).zfill(3)
    bnumstr = "{0}.{1}.{2}.{3}-{4}".format(majorversion, minorversion, revison, bnum, args.mode)
    print("build version: {0}".format(bnumstr))



    # build Integrated Presenter
    buildcmd_integratedpresenter = 'msbuild "IntegratedPresenter" -r /t:publish /p:PublishDir=".\..\publish\{0}\IntegratedPresenter" /p:Configuration=Release /p:SelfContained=true /p:RuntimeIdentifier=win-x64-aot /p:PublishSingleFile=true'.format(bnumstr)
    os.system(buildcmd_integratedpresenter) 
    # rename with build number
    cwd = os.getcwd()
    scpath = os.path.join(cwd, "publish", bnumstr, "IntegratedPresenter")
    print(scpath)
    os.rename("{0}\IntegratedPresenter.exe".format(scpath), "{0}\IntegratedPresenter_{1}_win_x64.exe".format(scpath, bnumstr))

    # increment build number
    build += 1
    version["Build"] = build
    version['Mode'] = "Debug"
    # save new build number
    with open('./IntegratedPresenter/version.json', "w") as f:
        json.dump(version, f)


if __name__ == '__main__':
    main()
