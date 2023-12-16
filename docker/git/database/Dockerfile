FROM postgres

RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

WORKDIR /docker-entrypoint-initdb.d

VOLUME  ["/etc/postgresql", "/var/log/postgresql", "/var/lib/postgresql/data"]