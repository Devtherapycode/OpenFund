# PostgreSQL Docker Setup

## Quick Start

### 1. Start PostgreSQL
```bash
docker-compose up -d
```

### 2. Verify PostgreSQL is Running
```bash
docker ps
```

You should see `openfund-postgres` container running.

### 3. Run Database Migrations
```bash
dotnet ef database update --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api
```

### 4. Run the Application
```bash
dotnet run --project src/OpenFund.Api
```

## PostgreSQL Connection Details

- **Host:** localhost
- **Port:** 5432
- **Database:** openfund_dev
- **Username:** openfund
- **Password:** openfund123

## Useful Docker Commands

### Stop PostgreSQL
```bash
docker-compose down
```

### Stop and Remove Data
```bash
docker-compose down -v
```

### View Logs
```bash
docker-compose logs -f postgres
```

### Connect to PostgreSQL CLI
```bash
docker exec -it openfund-postgres psql -U openfund -d openfund_dev
```

### Restart PostgreSQL
```bash
docker-compose restart
```

## Database Management

### Create a New Migration
```bash
dotnet ef migrations add MigrationName --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api
```

### Apply Migrations
```bash
dotnet ef database update --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api
```

### Reset Database (Drop and Recreate)
```bash
dotnet ef database drop --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api --force
dotnet ef database update --project src/OpenFund.Infrastructure --startup-project src/OpenFund.Api
```

## Troubleshooting

### Port 5432 Already in Use
If you have PostgreSQL installed locally, either:
1. Stop the local PostgreSQL service
2. Change the port in `docker-compose.yml`:
   ```yaml
   ports:
     - "5433:5432"
   ```
   And update `appsettings.Development.json`:
   ```json
   "Default": "Host=localhost;Port=5433;Database=openfund_dev;Username=openfund;Password=openfund123"
   ```

### Connection Refused
Make sure PostgreSQL container is running:
```bash
docker ps | grep openfund-postgres
```

If not running, start it:
```bash
docker-compose up -d
```
