---
title: "Docker LinqPad Driver"
categories:
  - Blog
tags:
  - docker
  - linqpad
---

So recently, I have been doing a lot of docker stuff.

- Creating docker containers on the fly, managing lifetime of containers.
- Creating docker image installer apps.
- General purpose docker builds

Now querying the docker images, and logs usually means logging into a portal such as Portainer, or using the Docker GUI (which is actually quite nice, if a little limited). So decided to create a docker linqpad driver, to keep querying in one place.

It can be added within LinqPad by

1. Clicking Add Connection in left connection pane
2. Add new drivers to bring up drivers dialog
3. Select View All Drivers
4. Install `Docker.LINQPadDriver`

Here is the [Nuget.org link](https://www.nuget.org/packages/Docker.LINQPadDriver/)

![Docker Install](/assets/images/install_docker_linqpad.png)

