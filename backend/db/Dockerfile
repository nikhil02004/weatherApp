FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN apt-get update && apt-get install -y dos2unix && rm -rf /var/lib/apt/lists/*

WORKDIR /init

# Copy the schema / stored-procedures script (context = backend root)
COPY Auth.Service/Auth.Service.Infrastructure/SQL/StoredProcedures.sql .

# Copy and normalise the entrypoint script (context = backend root)
COPY db/init.sh /init.sh
RUN dos2unix /init.sh && chmod +x /init.sh

USER mssql

EXPOSE 1433

ENTRYPOINT ["/bin/bash", "/init.sh"]
