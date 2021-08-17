pipeline{
    agent {
      label 'windows'
    }
    
    parameters {
      booleanParam defaultValue: true, description: '', name: 'Windows'
      booleanParam defaultValue: false, description: '', name: 'Mac'
      booleanParam defaultValue: true, description: '', name: 'Web'
//        choice choices: ['master', 'develop', 'feature/network'], description: '', name: 'BuildBranch'
    }

    stages { 
        stage('Checkout') {
            steps {
                checkout([$class: 'GitSCM', branches: [[name: "*/${env.BRANCH_NAME}"]], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'CloneOption', depth: 1, noTags: false, reference: '', shallow: true, timeout: 45]], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'b24266f2-2db3-4d10-a622-bbaefafca6cf', url: 'git@github.com:YuriAICruz/shmup-steam.git']]])
            }
        }
        stage('Build Windows') {
            when {
                expression { env.Windows == 'true' }
            }
            steps {
                bat '''
                    IF EXIST ./Builds (
                        RMDIR /S /Q Builds
                    )
                    
                    mkdir Builds
                    
                    %UNITY_19_4% -quit --stacktrace --info -logFile log.txt -batchmode -buildTarget Standalone -projectPath "%cd%" -executeMethod Builder.BuildWindows
                    
                    type log.txt
                '''
            }
        }
        stage('Upload Windows') {
            when {
                expression { env.Windows == 'true' }
            }
            steps {
                bat '''
                    %Butler% push Builds/StandaloneWindows64/ graphene-ai/shmup:windows-beta
                '''
            }
        }
        stage('Build Mac') {
            when {
                expression { env.Mac == 'true' }
            }
            steps {
                bat '''
                    IF EXIST ./Builds (
                        RMDIR /S /Q Builds
                    )
                    
                    mkdir Builds
                    
                    %UNITY_19_4% -quit --stacktrace --info -logFile log.txt -batchmode -buildTarget Standalone -projectPath "%cd%" -executeMethod Builder.BuildMac
                    
                    type log.txt
                '''
            }
        }
        stage('Upload Mac') {
            when {
                expression { env.Mac == 'true' }
            }
            steps {
                bat '''
                    %Butler% push Builds/StandaloneOSX/ graphene-ai/shmup:osx-beta
                '''
            }
        }
        stage('Build Web') {
            when {
                expression { env.Web == 'true' }
            }
            steps {
                bat '''
                    IF EXIST ./Builds (
                        RMDIR /S /Q Builds
                    )
                    
                    mkdir Builds
                    
                    %UNITY_19_4% -quit --stacktrace --info -logFile log.txt -batchmode -buildTarget Standalone -projectPath "%cd%" -executeMethod Builder.BuildWeb
                    
                    type log.txt
                '''
            }
        }
        stage('Upload Web') {
            when {
                expression { env.Web == 'true' }
            }
            steps {
                bat '''
                    %Butler% push Builds/WebGL/shmup-steam_Web/ graphene-ai/shmup:'web-beta'
                '''
            }
        }
    }
}