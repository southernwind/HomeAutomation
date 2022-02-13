node {
  stage('CheckOut'){
    checkout([$class: 'GitSCM', branches: [[name: '*/master']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]], submoduleCfg: [], userRemoteConfigs: [[url: 'https://github.com/southernwind/HomeAutomation']]])
  }

  stage('Build HomeAutomation'){
    dotnetBuild configuration: 'Release', project: 'HomeAutomation.sln', sdk: '.NET6', unstableIfWarnings: true
  }

  withCredentials( \
      bindings: [sshUserPrivateKey( \
        credentialsId: 'ac005f9d-9b4b-496f-873c-1c610df01c03', \
        keyFileVariable: 'SSH_KEY', \
        usernameVariable: 'SSH_USER')]) {
    stage('Deploy HomeAutomation'){
      sh 'scp -pr -i ${SSH_KEY} ./HomeAutomation/bin/Release/net6.0/* ${SSH_USER}@home-server.localnet:/opt/home-automation-service'
    }

    stage('Restart HomeAutomation'){
      sh 'ssh home-server.localnet -t -l ${SSH_USER} -i ${SSH_KEY} sudo service home_automation restart'
    }
  }

  stage('Notify Slack'){
    sh 'curl -X POST --data-urlencode "payload={\\"channel\\": \\"#jenkins-deploy\\", \\"username\\": \\"jenkins\\", \\"text\\": \\"HomeAutomationサービスのデプロイが完了しました。\\nBuild:${BUILD_URL}\\"}" ${WEBHOOK_URL}'
  }
}