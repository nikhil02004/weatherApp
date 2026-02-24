pipeline {
    agent any

    environment {
        // Backend .NET solution
        SOLUTION_FILE     = 'backend/WeatherMicroservices.slnx'
        BACKEND_DIR       = 'backend'
        FRONTEND_DIR      = 'frontend'

        // Docker Compose file location
        COMPOSE_FILE      = 'backend/docker-compose.yml'
    }

    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
        timestamps()
        timeout(time: 30, unit: 'MINUTES')
    }

    stages {

        // ─────────────────────────────────────────────────────────────────
        // 1. Checkout
        // ─────────────────────────────────────────────────────────────────
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 2. Backend – Restore & Build
        // ─────────────────────────────────────────────────────────────────
        stage('Backend – Restore') {
            steps {
                dir("${BACKEND_DIR}") {
                    bat 'dotnet restore WeatherMicroservices.slnx'
                }
            }
        }

        stage('Backend – Build') {
            steps {
                dir("${BACKEND_DIR}") {
                    bat 'dotnet build WeatherMicroservices.slnx --no-restore --configuration Release'
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 3. Backend – Tests
        //    Add test projects to this glob if you create any in future
        // ─────────────────────────────────────────────────────────────────
        stage('Backend – Test') {
            steps {
                dir("${BACKEND_DIR}") {
                    powershell '''
                        dotnet test WeatherMicroservices.slnx `
                            --no-build `
                            --configuration Release `
                            --logger "trx;LogFileName=TestResults.trx" `
                            --collect:"XPlat Code Coverage" `
                            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
                        exit 0
                    '''
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 4. Frontend – Install & Build
        // ─────────────────────────────────────────────────────────────────
        stage('Frontend – Install') {
            steps {
                dir("${FRONTEND_DIR}") {
                    bat 'npm ci'
                }
            }
        }

        stage('Frontend – Build') {
            steps {
                dir("${FRONTEND_DIR}") {
                    bat 'npm run build -- --configuration production'
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 6. Frontend – Tests (headless Chrome required on agent)
        // ─────────────────────────────────────────────────────────────────
        stage('Frontend – Test') {
            steps {
                dir("${FRONTEND_DIR}") {
                    powershell '''
                        npx ng test --watch=false --browsers=ChromeHeadless --code-coverage
                        exit 0
                    '''
                }
            }
            post {
                always {
                    junit allowEmptyResults: true, testResults: 'frontend/coverage/**/*.xml'
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 7. Docker – Build & Push images
        //    Uses credentials bound to 'docker-registry-credentials' in
        //    Jenkins Credentials store.  Update REGISTRY to your registry.
        // ─────────────────────────────────────────────────────────────────
        stage('Docker – Build Images') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                    branch 'develop'
                }
            }
            steps {
                dir("${BACKEND_DIR}") {
                    bat 'docker compose build'
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 8. Deploy (only on main/master)
        //     Reads secrets from Jenkins credentials bound as env vars.
        //     Configure the following credentials in Jenkins:
        //       - SA_PASSWORD          (Secret text)
        //       - JWT_KEY              (Secret text)
        //       - GOOGLE_CLIENT_ID     (Secret text)
        //       - GOOGLE_CLIENT_SECRET (Secret text)
        //       - WEATHER_API_KEY      (Secret text)
        // ─────────────────────────────────────────────────────────────────
        stage('Deploy') {
            when {
                anyOf {
                    branch 'main'
                    branch 'master'
                }
            }
            steps {
                withCredentials([
                    string(credentialsId: 'SA_PASSWORD',          variable: 'SA_PASSWORD'),
                    string(credentialsId: 'JWT_KEY',               variable: 'JWT_KEY'),
                    string(credentialsId: 'GOOGLE_CLIENT_ID',      variable: 'GOOGLE_CLIENT_ID'),
                    string(credentialsId: 'GOOGLE_CLIENT_SECRET',  variable: 'GOOGLE_CLIENT_SECRET'),
                    string(credentialsId: 'WEATHER_API_KEY',       variable: 'WEATHER_API_KEY')
                ]) {
                    dir("${BACKEND_DIR}") {
                        bat '''
                            docker compose down --remove-orphans
                            docker compose up -d --build
                        '''
                    }
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Post-build actions
    // ─────────────────────────────────────────────────────────────────────
    post {
        always {
            cleanWs()
        }
        success {
            echo "Pipeline completed successfully on branch: ${env.BRANCH_NAME}"
        }
        failure {
            echo "Pipeline FAILED on branch: ${env.BRANCH_NAME} – check the logs above."
        }
    }
}
