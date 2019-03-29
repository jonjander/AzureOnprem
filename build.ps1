$unityusername = "user.name@domain.ru"
$unitypassword = Read-Host
cd "C:\Git\VDC\VDC"

function unityusername {
    $unityusername
}

function unitypassword {
    $unitypassword
}

#use in azure devops 
$project = "\"
$arguments = @(
"-quit", 
"-batchmode", 
"-logFile stdout.log", 
"-projectPath $project", 
"-executeMethod GameBuilder.build"
"-username $(unityusername)",
"-password $(unitypassword)"
)
$processinfo = Start-Process -FilePath "C:\Program Files\Unity\Editor\Unity.exe" -ArgumentList $arguments -PassThru
Wait-Process -Id $processinfo.Id