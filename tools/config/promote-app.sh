#!/bin/bash
BUILD_NAMESPACE=$1
APP=$2
BUILD_TAG=$3
ENV_PREFIX=$4
BUILD_IS_TAG=$APP:$BUILD_TAG
ENV_IS_TAG=$APP:$ENV_PREFIX
LATEST_TAG=$APP:latest 
echo "+\n++ API: Deploying latest tag $BUILD_TAG or latest of $APP with ENV $ENV_PREFIX \n+"
if oc -n $BUILD_NAMESPACE  get istag $BUILD_IS_TAG;
then
  echo "\n++ Using Commit tag $BUILD_IS_TAG for Env $ENV_PREFIX\n+"
  oc -n $BUILD_NAMESPACE tag $BUILD_IS_TAG  $ENV_IS_TAG
else
  echo "\n++ Commit tag  $BUILD_TAG Not available using latest tag for Env $ENV_PREFIX \n+"
  oc -n $BUILD_NAMESPACE tag $LATEST_TAG $ENV_IS_TAG
fi

