# SaintSender
**SaintSender** is an **email manager** desktop application for Windows. It asks the user to login with their **Gmail** account, then provides them with functionalities like email sending, reading, archiving and deleting.

The main goal of this project was to practice **WPF** development using the **MVVM** design pattern. 

## How to run it
1. Clone the repository.
2. Compile the project with .NET Framework 4.7 or greater.
3. Open the `SaintSender.DesktopUI\bin\Debug` folder and run _SaintSender.DesktopUI.exe_.

## How to login to your Gmail account with SaintSender
To be able to use the features of this applications, you need to [authorize](https://myaccount.google.com/lesssecureapps) **unverified third-party apps** to have access to your Gmail account. After that, you will be able to login with SaintSender using your Gmail credentials. You can read more about third-party apps and their access level to your account [here](https://support.google.com/accounts/answer/3466521).

## GemBox.Email limitations
The IMAP client currently in use is a **freeware version** of [GemBox.Email](https://www.gemboxsoftware.com/email). If you own a license key for the Professional version, you can use it instead of the `FREE-LIMITED-KEY` provided by default. Alternatively, you might request a limited-time, provisional license key from [GemBox support](https://www.gemboxsoftware.com/email/support).

Please note that the [free version](https://www.gemboxsoftware.com/email/free-version) has the following limitations:
* Maximum size of a message attachment is 50 KB.
* Maximum number of sent, received, saved, and loaded messages is 50.

For a better experience, it is **highly recommended** to use a Gmail account which newest messages has up to 50 KB attachments when using the free version.

## Contributors
SaintSender was developed by:
* [J�nos Krizs�n](https://github.com/JanosKrizsan)
* [Mil�n Gergics](https://github.com/gergicsmilan)
* [D�vid Schmidt](https://github.com/DavidAdamSchmidt)

