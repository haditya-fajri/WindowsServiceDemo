# WindowsServiceDemo
Create Windows Service using .Net Core

For Deploy
:: Create a Windows Service
sc create DemoService DisplayName="Demo Service" binPath="C:\full\path\to\Demo.exe"

:: Start a Windows Service
sc start DemoService

:: Stop a Windows Service
sc stop DemoService

:: Delete a Windows Service
sc delete DemoService
