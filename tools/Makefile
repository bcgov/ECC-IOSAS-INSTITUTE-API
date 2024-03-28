-include ../.env
export $(shell sed 's/=.*//' ../.env)

.DEFAULT_GOAL := default
export GIT_LOCAL_BRANCH?=$(shell git rev-parse --abbrev-ref HEAD)
export APP_NAME := $(or $(PROJECT_NAME), ecc-iosas-d365-institute)
export BUILD_ID := $(or $(BUILD_ID), 1)
export TEMPLATE_FILE_NAME := $(or $(TEMPLATE_FILE_NAME), $$FILE)
export BUILD_REF := $(or $(BUILD_REF), $(GIT_LOCAL_BRANCH))
export BUILD_NAMESPACE := $(or $(BUILD_NAMESPACE), d1273d-tools)
export DOMAIN := $(or $(DOMAIN), apps.silver.devops.gov.bc.ca)
export ENV_PREFIX := $(or $(ENV_PREFIX), dev)
export NEW_DB := $(or $(NEW_DB), $$NEW_DB)
export JOB_NAME := $(or $(JOB_NAME), $$JN)
export SITE_MINDER_LOGOUT_URL := $(or $(SITE_MINDER_LOGOUT_URL), https://logontest.gov.bc.ca/clp-cgi/logoff.cgi)
export UI_PRIVATE_KEY := $(or $(UI_PRIVATE_KEY), None)
export UI_PUBLIC_KEY := $(or $(UI_PUBLIC_KEY), None)
export LAST_COMMIT := $(shell git log -1 --oneline --decorate=full --no-color --format="%h, %cn, %f, %D" | sed 's/->/:/')
export COMMIT_SHA:=$(shell git rev-parse --short=7 HEAD)
export APP_CONFIG_DEV := $(or $(APP_CONFIG_DEV), na)
export APP_CONFIG_TEST := $(or $(APP_CONFIG_TEST), na)
export APP_CONFIG_PROD := $(or $(APP_CONFIG_PROD), na)
export NAMESPACE_DEV := $(or $(NAMESPACE_DEV), d1273d-dev)
export NAMESPACE_TEST := $(or $(NAMESPACE_TEST), d1273d-dev)
export NAMESPACE_PROD := $(or $(NAMESPACE_PROD), d1273d-dev)

export REPO_NAME:=ECC-IOSAS-INSTITUTE-API


MAX_CPU=100m
MIN_CPU=50m
MAX_MEMORY=128Mi
MIN_MEMORY=128Mi

# Seeting up Name space as per env prefix
ifeq ($(ENV_PREFIX), dev)
NAMESPACE=$(NAMESPACE_DEV)
APP_CONFIG=$(APP_CONFIG_DEV)
MIN_REPLICA=2
MAX_REPLICA=2
endif

ifeq ($(ENV_PREFIX), test)
NAMESPACE=$(NAMESPACE_TEST)
APP_CONFIG=$(APP_CONFIG_TEST)
MIN_REPLICA=2
MAX_REPLICA=2
endif

ifeq ($(ENV_PREFIX), production)
NAMESPACE=$(NAMESPACE_PROD)
APP_CONFIG=$(APP_CONFIG_PROD)
MIN_REPLICA=3
MAX_REPLICA=3
endif


define BUILD_TAG
"$(BUILD_REF)-$(COMMIT_SHA)"
endef

define BUILD_TEMPLATE_PATH
"openshift/docker-build.yml"
endef

define API
"api-$(APP_NAME)"
endef

define Web
"web-$(APP_NAME)"
endef

define HOST
"$(ENV_PREFIX)-ecc-iosas-d365.$(DOMAIN)"
endef


define SOURCE_REPOSITORY_REF
"main"
endef

define rollout_and_wait
@oc -n $(NAMESPACE) rollout status $1
endef

current_datetime := $(shell date '+%Y%m%d%H%M%S')

# Not required
init-oc: | print-status init-project
# Build api container image with logging
oc-build-api: | print-status build-api
# Deploy api container using deployment config with logging
oc-deploy-api: | print-status deploy-api








# Logging
print-status:
	@echo " +---------------------------------------------------------+ "
	@echo " | Current Settings                                        | "
	@echo " +---------------------------------------------------------+ "
	@echo " | PROJECT:      			$(APP_NAME)"
	@echo " | BRANCH:       			$(BUILD_REF)"
	@echo " | NAMESPACE:    			$(NAMESPACE)"
	@echo " | BUILD_NAMESPACE:    $(BUILD_NAMESPACE)"
	@echo " +---------------------------------------------------------+ "
	@echo " | ENV: 				$(ENV_PREFIX) "
	@echo " | BUILD_ID: 	$(COMMIT_SHA) "
	@echo " | BUILD_TAG: 	$(BUILD_TAG) "
	@echo " | HOST NAME: 	$(HOST) "
	@echo " | APP CONFIG:"
	@echo '$(APP_CONFIG)'
	@echo " +---------------------------------------------------------+ "


# Create certificate for Dot Net API server
api-create-certificate:
	@echo "\n++ Creating service certificate ++\n"
	@openssl genpkey -algorithm RSA -out server.key
	@echo "Creating csr....\n"
	@openssl req -new -key ./server.key -out ./server.csr -config openssl.conf
	@echo "Creating crt....\n"
	@openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt
	@echo "++\n Init tls certificate secret in $(NAMESPACE) \n++"
	@oc -n $(NAMESPACE) delete secret  $(API)-$(ENV_PREFIX)-tls || true
	@oc -n $(NAMESPACE) create secret tls $(API)-$(ENV_PREFIX)-tls --cert=server.crt --key=server.key
	@rm server.key server.csr server.crt
	@echo "\n ++ certificate creation completed ++\n"


# Initializing api configMap with `APP_CONFIG` enviornment variable which will be saved as appsetting.json inside api container. APP_CONFIG will be created from Github variable
init-api:
	@echo "++\n Init Api in $(NAMESPACE) \n++"
	@oc -n $(NAMESPACE) delete configmap $(API)-$(ENV_PREFIX)-config || true
	@oc -n $(NAMESPACE) create configmap $(API)-$(ENV_PREFIX)-config --from-literal=appsettings.json='$(APP_CONFIG)'


# Not required
init-artifactory-secret-credential:
	test -n $(NAMESPACE)
	@oc create secret docker-registry artifactory-secret-credential -n $(NAMESPACE) \
    	--docker-server=artifacts.developer.gov.bc.ca \
    	--docker-username=$(USERNAME) \
    	--docker-password=$(PASSWORD) \
    	--docker-email=$(USERNAME)@$(NAMESPACE).local
	@oc secrets link default artifactory-secret-credential
	@oc secrets link builder artifactory-secret-credential

# Build Dot Net API image on OpenShift tools namespace using build config file (/tools/openshift/docker-build.yml)
build-api:
	test -n "$(BUILD_REF)"
	test -n "$(BUILD_NAMESPACE)"
	test -n "${API}"
	@echo "+\n++ BUILDING API with tag: $(BUILD_TAG)\n+"
	@oc -n $(BUILD_NAMESPACE) process -f $(BUILD_TEMPLATE_PATH) \
		-p TAG=$(BUILD_TAG) \
		-p SOURCE_REPOSITORY_REF=$(BUILD_REF) \
		-p BASE_IMAGE_NAME="nodejs-18" \
		-p BASE_IMAGE_TAG="1" \
		-p BASE_IMAGE_REPO="registry.access.redhat.com/ubi8/" \
		-p SOURCE_CONTEXT_DIR=/ -p NAME=$(API) | oc -n $(BUILD_NAMESPACE) apply -f -
	@oc -n $(BUILD_NAMESPACE) start-build bc/$(API) --wait --follow=true
	@oc -n $(BUILD_NAMESPACE) tag $(API):$(BUILD_TAG) $(API):latest
	@oc -n $(BUILD_NAMESPACE) tag $(API):$(BUILD_TAG) $(API):$(current_datetime)


# Update deployment config
update-dc-api:
	test -n "$(NAMESPACE)"
	test -n "$(BUILD_NAMESPACE)"
	@echo "+\n++ Updating API-dc with tag: $(BUILD_TAG)\n+"
	@oc -n $(NAMESPACE) process -f openshift/api-deploy.yml \
		-p NAME=$(API) \
		-p PREFIX=$(ENV_PREFIX) \
		-p BUILD_NAMESPACE=$(BUILD_NAMESPACE) \
		-p IMAGE_STREAM_TAG="$(API):$(ENV_PREFIX)" \
		-p BUILD_ID=$(BUILD_TAG) \
		-p CPU_LIMIT=$(MAX_CPU) \
		-p CPU_REQUEST=$(MIN_CPU) \
		-p MEMORY_REQUEST=$(MIN_MEMORY) \
		-p MEMORY_LIMIT=$(MAX_MEMORY) \
		-p REPLICAS=$(MIN_REPLICA) | oc -n $(NAMESPACE) apply -f -

# Deployment rollout the API container
deploy-api: update-dc-api
	@oc rollout cancel dc/$(API) 2> /dev/null || true && echo "No rollout in progress"
	@oc -n $(NAMESPACE) rollout status dc/$(API) --watch=true

# Promoting imange on diff OpenShift namespaces 
promote-api:
	test -n "$(BUILD_NAMESPACE)"
	echo "+\n++ API: Deploying latest tag $(BUILD_TAG) API with ENV $(ENV_PREFIX) $\n+"
	sh config/promote-app.sh $(BUILD_NAMESPACE) $(API) $(BUILD_TAG) $(ENV_PREFIX)

# OpenShift Port Forwarding tunnel
d365-api-tunnel: print-status
	@echo "+\n++ Port Forwarding to D365 stack"
	@oc -n $(NAMESPACE) port-forward svc/$(API) 5091


# Tagging build in github
tag-commit:
	@echo "Tagging commit with date-time: $(current_datetime)"
	@git tag -fa $(ENV_PREFIX)-$(current_datetime) -m "$(ENV_PREFIX): Tagging the deployment"
	@git push origin --tags
tag-test:
ifdef ticket
	@git tag -fa test -m "Deploy $(ticket) to TEST Openshift env"
else
	@echo -e '\nTicket name missing - Example :: make tag-test ticket=HCAP-ABC \n'
	@echo -e 'Falling Back to using branch name\n'
	@git tag -fa test -m "Deploy $(git rev-parse --abbrev-ref HEAD) to TEST env"
endif
	@git push --force origin refs/tags/test:refs/tags/test

tag-production:
ifdef ticket
	@git tag -fa production -m "Deploy $(ticket) to PROD Openshift env"
else
	@echo -e '\nTicket name missing - Example :: make tag-test ticket=HCAP-ABC \n'
	@echo -e 'Falling Back to using branch name\n'
	@git tag -fa production -m "Deploy $(git rev-parse --abbrev-ref HEAD) to PROD env"
endif
	@git push --force origin refs/tags/production:refs/tags/production


