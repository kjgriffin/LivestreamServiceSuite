import json
import os

def main():
    print("starting builds...")
    with open ('./version.json') as f:
        version = json.load(f)
    # get build numbers
    majorversion = version['MajorVersion']
    minorversion = version['MinorVersion']
    revison = version['Revision']
    build = version['Build']
    # increment build number
    build += 1

    version["Build"] = build

    print(version)

    # save new build number
    with open ('./version.json', "w") as f:
        json.dump(version, f)

    # create build number string
    bnum = str(build).zfill(3)
    bnumstr = "{0}.{1}.{2}.{3}".format(majorversion, minorversion, revison, bnum)
    print("build version: {0}".format(bnumstr))

    # build SlideCreater
    buildcmd_slidecreater = 'dotnet publish SlideCreater -c Release -p:PublishDir=.\..\publish\{0}\SlideCreater -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true -r win-x64-aot'.format(bnumstr)
    os.system(buildcmd_slidecreater) 
    # rename with build number
    cwd = os.getcwd();
    scpath = os.path.join(cwd, "publish", bnumstr, "SlideCreater")
    print(scpath)
    os.rename("{0}\SlideCreater.exe".format(scpath), "{0}\SlideCreater_{1}_win_x64.exe".format(scpath, bnumstr))


    # build Integrated Presenter
    buildcmd_integratedpresenter = 'msbuild "Integrated Presenter" -r /t:publish /p:PublishDir=".\..\publish\{0}\IntegratedPresenter" /p:Configuration=Release /p:SelfContained=true /p:RuntimeIdentifier=win-x64-aot /p:PublishSingleFile=true'.format(bnumstr)
    os.system(buildcmd_integratedpresenter) 
    # rename with build number
    cwd = os.getcwd();
    scpath = os.path.join(cwd, "publish", bnumstr, "IntegratedPresenter")
    print(scpath)
    os.rename("{0}\Integrated Presenter.exe".format(scpath), "{0}\IntegratedPresenter_{1}_win_x64.exe".format(scpath, bnumstr))



if __name__ == '__main__':
    main()
