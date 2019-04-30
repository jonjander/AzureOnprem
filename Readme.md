<<<<<<< HEAD
# Downloads
[Downloads ![Download](https://material.io/tools/icons/static/icons/baseline-cloud_download-24px.svg)](https://azureonprem.azurewebsites.net/releases/)
=======
# Project AzureOnprem
This project is a fun demonstation of how one can use Azure Resource Management API's. The project started of as a joke but turned into something bigger. If you like me work with Azure and also have Unity and/or C# experiance, please feel free to contribute to this project or just share your thoughts and ideas.

## prerequisites
* Microsoft Azure subscription
* At least read access to one resource group

## Security
This code will issue an user access token to "https://management.azure.com/" on your behaf to be able to read azure resource information. Even though the game only reads information, I highly recommend to use a read only or a low permission account when signing in with your account to programs you don't have source code access to or dont understand the code.
>>>>>>> origin/jonjander-patch-readme

# How to build
1. Build **.\VDC.Login\VDC.Login.sln**
2. Copy the **.\VDC.Login\VDC.Login\bin\Debug** files to **.\VDC\Assets\StreamingAssets**
3. Open the project in Unity 2018.3
4. Build the game for Windows

## Limitaitons
The login uses the vdc.login.exe as a streeming content therfore this will only work on windows systems. You will still be able to launch the game on other systems but the data center generation and login will fail.

# Automated builds

## Build status
[![Build status](https://lanmat.visualstudio.com/AzureOnprem/_apis/build/status/AzureOnprem-CI)](https://lanmat.visualstudio.com/AzureOnprem/_build/latest?definitionId=6)

## Release 
[![Release status](https://lanmat.vsrm.visualstudio.com/_apis/public/Release/badge/e95634b1-2d88-4006-b4f2-26793e1c5ec5/1/1)](https://lanmat.visualstudio.com/AzureOnprem/_release?view=all&definitionId=1)

# Change log
[Changelog.md](ChangeLog.md)

