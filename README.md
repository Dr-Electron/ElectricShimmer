# ElectricShimmer

![](https://raw.githubusercontent.com/Dr-Electron/ElectricShimmer/master/Images/Banner.svg)

This is a GUI for the GoShimmer CLI wallet in C#. It is used in the [GoShimmer Testnet](https://github.com/iotaledger/goshimmer).  
The program will automatically download the latest cli-wallet, if there isn't already one in the same directory.

## Config File
If you need some extra settings you can add a config file named `config.toml`
At the moment it contains two possible settings.
Example content for the config file:
```
[AutoUpdate]
enabled = false

[Logger]
LogLevel = "ALL"
```
With `[AutoUpdate] enabled` you can disable AutoUpdate for ElectricShimmer.
And `[Logger] LogLevel` is used to specify the LogLevel. Every LogLevel activates itself and all lower levels. 
Possible Values:
```
ALL,
INFO,
ERROR,
EXCEPTION,
OFF
```
