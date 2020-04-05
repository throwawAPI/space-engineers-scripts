# ThrowawAPI's Space Engineer Scripts

I offer a collection of ingame scripts for Space Engineers' servers that other players may find helpful. I have worked to solve three issues when publishing these scripts:

1) I want these scripts to be as organized as is reasonable. There's a reasonable amount of reusable "stub" code in SE, so I have packaged that into scripts available in the [Template directory](Template/). We softlink to particular versions of the template and boilerplate code rather than linking to the most recent template. This is done so that code is locked to a template version and breaking changes should not be accidently introduced to older code by adding template features.

2) These scripts are packaged and will be [released](https://github.com/throwawAPI/space-engineers-scripts/releases) with versioning matching the [Semantic Versioning 2.0.0 protocol](https://semver.org/). If a script is released prior to version 1.0, this is a test release and the surrounding API and codebase may not be stable. Code releases will be packaged as a single zipped .cs (C Sharp) file, which can be added directly to your Space Engineers' ingame folder, available at `%appdata%\SpaceEngineers\IngameScripts` for Windows users.

3) I don't like Windows, Visual Studio, Microsoft's C# .Net kit, or any of it really. I want to be able to write C# how I prefer, without being bound to a single file's size by Space Engineers' limitations. This entire project, especially the packing scripts, are to circumvent Microsoft's near stranglehold on C# development in a Windows environment. If you like Visual Studio, I cannot recommend [malware-dev's excellent MDK-SE Visual Studio tools](https://github.com/malware-dev/MDK-SE) or [his Space Engineers API](https://github.com/malware-dev/MDK-SE/wiki/Api-Index) enough. Regardless of whether you use his tools or mine, the API linked here is unbelievably useful and robust.

## Getting Started

If you're just looking for a script, grab it from the [releases](https://github.com/malware-dev/MDK-SE/wiki/Api-Index) tab. If you're looking to fork or contribute, check the build instructions below.

### Prerequisites

You need a development environment, but do *not* need the ability to run C# code on your own. SE will handle that for us. I use Atom and gvim, but any reasonable text editor will do.

## Testing

Unfortunately, I do not have a mechanism in place to test code automatically, as it depends on the SE API, which seems to be buried within Space Engineers. If I come up with a clever way to pack and test scripts automatically, without needing ingame Programming Blocks, I'll be sure to let you know 😅

### Coding Style

I feel that these tests will be more easily enforced, and will update this section when I have a style guide ready.

## Deployment

So, you've edited my code, and you're ready to compile these partial C# scripts into one big script to receive [Clang's judgement](https://www.reddit.com/r/spaceengineers/comments/4e3voo/what_is_clang/d1x3fk5/) *(editor's note: I am personally a skeptic of Clang, but I think it makes a good meme. Just learn how to use pistons, center of mass, and gyroscopes properly, guys.)* We're going to run one line of code, and then we're ready to go. Run either `pack.bat` for Windows, or `pack.sh` in Unix shell compatible environments, whichever is easier for you. Pack your `Script.cs` file like this:

```
pack.sh Battery-Monitor/Battery-Monitor_v1.0
```

The script will collect all `_*.cs` files (known as partials), and assemble them into `Script.cs`. If an existing `Script.cs` would be overwritten, this existing file is copied into `.Script.old.cs`. Should anything go horrendously wrong, you have a backup file in that directory. You're welcome 🙃

## Contributing

Please read [CONTRIBUTING.md](docs/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/throwawAPI/space-engineers-scripts/tags).

## Authors

* **Grant Sparks** - *Primary Contributor* - [throwawAPI](https://github.com/throwawAPI)

See also the list of [contributors](https://github.com/throwawAPI/space-engineers-scripts/contributors) who participated in this project.

## License

I have not yet decided on an appropriate license for this project - see the [LICENSE.md](LICENSE.md) file for details
