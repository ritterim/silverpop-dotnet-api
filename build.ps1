New-Item -ItemType directory -Path "build" -Force | Out-Null

# The following variables should be set to true if unit tests need a Docker container created
#$ENV:RIMDEV_CREATE_TEST_DOCKER_ES = "false" # Elasticsearch on 9201/9301

#$ENV:RIMDEV_CREATE_TEST_DOCKER_SQL = "true" # MS SQL on 11434
#$ENV:RIMDEVTESTS__SQL__HOSTNAME = "localhost"
#$ENV:RIMDEVTESTS__SQL__PORT = "11435"
#$ENV:RIMDEVTESTS__SQL__PASSWORD = "6Bx3mmLvml6j"

try {
  Invoke-WebRequest https://raw.githubusercontent.com/ritterim/build-scripts/master/bootstrap-cake.ps1 -OutFile build\bootstrap-cake.ps1
  #Invoke-WebRequest https://raw.githubusercontent.com/ritterim/build-scripts/master/build-net5.cake -OutFile build.cake
}
catch {
  Write-Output $_.Exception.Message
  Write-Output "Error while downloading shared build script, attempting to use previously downloaded scripts..."
}

.\build\bootstrap-cake.ps1 -Verbose --verbosity=Normal

Exit $LastExitCode
