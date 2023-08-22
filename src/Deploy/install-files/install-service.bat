nssm install BosmanCommerce7 BosmanCommerce7.Blazor.Server.exe
nssm set BosmanCommerce7 AppDirectory %~dp0
nssm set BosmanCommerce7 DisplayName Bosman Commerce7 Sync
nssm set BosmanCommerce7 Description This is the Bosman Commerce7 Sync service.
nssm set BosmanCommerce7 AppRestartDelay 5000
