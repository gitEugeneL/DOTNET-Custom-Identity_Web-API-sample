FROM postgres:18

COPY ./init_database.sql /docker-entrypoint-initdb.d/init_database.sql 
RUN chmod 644 /docker-entrypoint-initdb.d/*.sql