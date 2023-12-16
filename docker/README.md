# Build

To build a project for prod, you need to fill in the data in 'prod.env' and specify it with the docker-compose file

```cmd
docker-compose --file docker-compose.yml --env-file dev.env build
```

# Run

```cmd
docker-compose --file docker-compose.yml --env-file dev.env up
```