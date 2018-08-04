# README
### Please read the accompanying wiki page
Please make sure you have read the wiki page after you have completed compiling the sources.

## Compiling the source

1. Open the solution folder in Visual Studio 2017. (The targetted .Net framework is 4.5.2.)
1. Nuget will ensure that all packages are downloaded before compilation.
1. This project currently uses MySQL as the database server. The schema script file for the required is included in the ObjectStore\Persistency.Adaptors\MySql\ folder ("Schema.txt").
1. Make sure you run this database script to create the database on your local (or even remote) server.
1. Open the TestBase.cs file in the ObjectStore.Tests\Tests\ folder. Add proper credentials of your database instance in the AppConfigSimulator method for the following properties: Host, PortNo, DbName, Username, Password. These reporesent, in the same order, your MySQL database host, MySQL port, the database name, your MySql database username so that you able to access the database, and the password.
1. Leave the PersistenceDriver property as is.
1. Compile the project.
1. Make sure you have NUnit installed on your development machine.
1. Once the compilation completes successfully, open the file ObjectStore.Tests.nunit in the ObjectStore.Tests\ project folder.
1. You will be able to view the test suite in the NUnit runner.
1. Run the entire test suite by pressing F5.
1. Hopefully you should see a green bar after the test suite completes running all the tests. This shouldn't take more than a few seconds.
1. Read the accompanying wiki page.
