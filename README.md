## About

A Fork of CsPosh that uses a custom parser in order to Covenant Friendly and allow for spaces in commands.

Origin Repo: https://github.com/rasta-mouse/MiscTools/tree/master/CsPosh

### Differences

Covenant's `Assembly` module splits on spaces and passes the array as arguments to the assembly. This means that quotes cannot be used to include spaces in arguments. Therefore, in order to allow the use of spaces, this project uses a custom parser that rejoins arguments and leverages a `specialkeyword=some value` format to pass arguments.

```
Usage:
    target=somehost code=any powershell command stuff
    target=somehost encoded=Base64EncodedPowershellBlah
    target=somehost code=any powershell command stuff domain=SomeDomain username=somusername password=somepassword outstring=false
```
