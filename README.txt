How-To compile the Bot:

Windows:
* Install .Net Framework 2.0 or Mono
* Install MySQL® Connector/Net 5.1
* Install MySQL 5.1
* Compile the source using Sharp Develop, MS Visual Studio or Mono
* Setup the initial MySQL Database using the MySQL-Dump in SQL/mysql.sql
* Copy the cs-elbot.exe.config into the folder with your binaries (bin/Release or bin/Debug)
* Modify the cs-elbot.exe.config according your needs
* Execute the cs-elbot.exe

Linux:
* Install Mono (Make sure that you have also installed the gmcs compiler)
* Install MySQL® Connector/Net 5.1
* Install MySQL 5.1
* Copy the MySql.Data.dll to the root directory of the sources
* run make
* Setup the initial MySQL Database using the MySQL-Dump in SQL/mysql.sql
* Copy the cs-elbot.exe.config into the folder with your binaries (bin/Release or bin/Debug)
* Modify the cs-elbot.exe.config according your needs
* Execute the cs-elbot.exe (mono cs-elbot.exe)