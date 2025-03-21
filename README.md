<p align="center">
  <img src="src/assets/apps/default.png" width="100" alt="Logo" >
</p>

<h1 align="center">Home App</h1>

<p align="center">
<a href="https://github.com/citr0s/home-app/actions/workflows/build.yml"><img src="https://github.com/citr0s/home-app/actions/workflows/build.yml/badge.svg" alt="Build"></a>
<a href="https://github.com/citr0s/home-app/actions/workflows/deploy.yml"><img src="https://github.com/citr0s/home-app/actions/workflows/deploy.yml/badge.svg" alt="Publish Docker image"></a>
<a href="https://hub.docker.com/r/citr0s/home-app"><img src="https://img.shields.io/docker/image-size/citr0s/home-app" alt="Docker Image Size"></a>
<a href="https://hub.docker.com/r/citr0s/home-app"><img src="https://img.shields.io/docker/pulls/citr0s/home-app" alt="Docker pulls"></a>
<a href="https://hub.docker.com/r/citr0s/home-app"><img src="https://img.shields.io/docker/v/citr0s/home-app?sort=semver" alt="Docker version"></a>
</p>

---

<h4 align="center">Home App is a self-hosted web app designed to provide you with better dashboard for a homelab.</h4>

---

> [!WARNING]
> The app is still actively in development. Only latest release is available.
> Once stable, a release tag will be made available.

---

## 🛠️ Installation

> [!NOTE]
> To run this application, you'll need [Docker](https://docs.docker.com/engine/install/) with [docker-compose](https://docs.docker.com/compose/install/).

Start off by showing some ❤️ and give this repo a star. Then from your command line:

```bash
# Create a new directory
> mkdir home-app
> cd home-app

# Create docker-compose.yml and copy the example contents into it
> touch docker-compose.yml
> nano docker-compose.yml
```

### docker-compose.yml

```yml
services:

    home-app:
        image: citr0s/home-app
        ports:
            - '82:80'
        environment:
            - ASPNETCORE_ENVIRONMENT=Production
            - ASPNETCORE_URLS=http://+:80
            - ASPNETCORE_CONNECTION_STRING=host=${POSTGRES_HOST};port=5432;database=home_app;username=${POSTGRES_USER};password=${POSTGRES_PASSWORD};Pooling=true;
            - ASPNETCORE_MINIO_ENDPOINT=${MINIO_HOST}
            - ASPNETCORE_MINIO_ACCESS_KEY=${MINIO_ACCESS_KEY}
            - ASPNETCORE_MINIO_SECRET_KEY=${MINIO_SECRET_KEY}
            - ASPNETCORE_MINIO_BUCKET_NAME=${MINIO_BUCKET_NAME}
            - ASPNETCORE_MINIO_CDN_URL=${MINIO_CDN_URL}
            - ASPNETCORE_WEATHER_API_KEY=${WEATHER_API_KEY}
            - ASPNETCORE_MAPS_API_KEY=${MAPS_API_KEY}
        volumes:
            - ./app-data:/data
            - ./letsencrypt:/etc/letsencrypt
        depends_on:
            - postgres
            - minio

    postgres:
        image: postgres:15.12
        environment:
            - POSTGRES_USER=postgres
            - POSTGRES_PASSWORD=postgres
        ports:
            - "5432:5432"
        volumes:
            - ./postgres-data:/var/lib/postgresql/data/

    minio:
        image: minio/minio:latest
        ports:
            - "9000:9000"
            - "9002:9001"
        environment:
            - MINIO_ROOT_USER=root_user
            - MINIO_ROOT_PASSWORD=root_password
        command: server --address ":9000" --console-address ":9001" /data
        volumes:
            - ./minio-data/minio:/data
```

---

## 💡 Feature request?

For any feedback, help or feature requests, please [open a new issue](https://github.com/citr0s/home-app/issues/new/choose).
Before you do, please read [the wiki](https://github.com/citr0s/home-app/wiki). The question you have might be answered over there.