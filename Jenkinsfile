pipeline {
    agent any

    environment {
        // SonarQube server name configured in Jenkins → Manage Jenkins → Configure System
        SONAR_SERVER      = 'SonarQube'

        // Backend .NET solution
        SOLUTION_FILE     = 'backend/WeatherMicroservices.slnx'
        BACKEND_DIR       = 'backend'
        FRONTEND_DIR      = 'frontend'

        // SonarQube project keys (must match what was used locally)
        SONAR_BACKEND_KEY = 'weather-backend'
        SONAR_FRONTEND_KEY= 'weather-frontend'

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
                    sh 'dotnet restore WeatherMicroservices.slnx'
                }
            }
        }

        stage('Backend – Build') {
            steps {
                dir("${BACKEND_DIR}") {
                    sh 'dotnet build WeatherMicroservices.slnx --no-restore --configuration Release'
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
                    sh '''
                        dotnet test WeatherMicroservices.slnx \
                            --no-build \
                            --configuration Release \
                            --logger "trx;LogFileName=TestResults.trx" \
                            --collect:"XPlat Code Coverage" \
                            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover \
                            || true
                    '''
                }
            }
            post {
                always {
                    // Publish MSTest/TRX results
                    step([$class: 'MSTestPublisher', testResultsFile: 'backend/**/TestResults.trx', failOnError: false])
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 4. Backend – SonarQube Analysis
        //    Requires: dotnet-sonarscanner tool installed on the agent
        //    Install once:  dotnet tool install --global dotnet-sonarscanner
        // ─────────────────────────────────────────────────────────────────
        stage('Backend – SonarQube Analysis') {
            steps {
                withSonarQubeEnv("${SONAR_SERVER}") {
                    dir("${BACKEND_DIR}") {
                        sh """
                            dotnet sonarscanner begin \
                                /k:"${SONAR_BACKEND_KEY}" \
                                /n:"WeatherApi Backend" \
                                /d:sonar.host.url="${SONAR_HOST_URL}" \
                                /d:sonar.token="${SONAR_AUTH_TOKEN}" \
                                /d:sonar.cs.opencover.reportsPaths="**/**/coverage.opencover.xml"

                            dotnet build WeatherMicroservices.slnx --configuration Release --no-restore

                            dotnet sonarscanner end \
                                /d:sonar.token="${SONAR_AUTH_TOKEN}"
                        """
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 5. Frontend – Install & Build
        // ─────────────────────────────────────────────────────────────────
        stage('Frontend – Install') {
            steps {
                dir("${FRONTEND_DIR}") {
                    sh 'npm ci'
                }
            }
        }

        stage('Frontend – Build') {
            steps {
                dir("${FRONTEND_DIR}") {
                    sh 'npm run build -- --configuration production'
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 6. Frontend – Tests (headless Chrome required on agent)
        // ─────────────────────────────────────────────────────────────────
        stage('Frontend – Test') {
            steps {
                dir("${FRONTEND_DIR}") {
                    sh '''
                        npx ng test \
                            --watch=false \
                            --browsers=ChromeHeadless \
                            --code-coverage \
                            || true
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
        // 7. Frontend – SonarQube Analysis
        //    Requires: sonar-scanner CLI installed on the agent or use
        //    the SonarQube Scanner Jenkins plugin (configured in tools)
        // ─────────────────────────────────────────────────────────────────
        stage('Frontend – SonarQube Analysis') {
            steps {
                withSonarQubeEnv("${SONAR_SERVER}") {
                    dir("${FRONTEND_DIR}") {
                        sh """
                            sonar-scanner \
                                -Dsonar.projectKey=${SONAR_FRONTEND_KEY} \
                                -Dsonar.projectName="WeatherApi Frontend" \
                                -Dsonar.sources=src \
                                -Dsonar.exclusions="**/node_modules/**,**/dist/**,**/.angular/**,**/*.spec.ts" \
                                -Dsonar.tests=src \
                                -Dsonar.test.inclusions="**/*.spec.ts" \
                                -Dsonar.host.url="${SONAR_HOST_URL}" \
                                -Dsonar.token="${SONAR_AUTH_TOKEN}"
                        """
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 8. Quality Gate – block the pipeline if Sonar gate fails
        // ─────────────────────────────────────────────────────────────────
        stage('Quality Gate') {
            steps {
                timeout(time: 5, unit: 'MINUTES') {
                    waitForQualityGate abortPipeline: true
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 9. Docker – Build & Push images
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
                    sh 'docker compose build'
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 10. Deploy (only on main/master)
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
                        sh '''
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
