until echo > /dev/tcp/postgres/5432; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 5
done

>&2 echo "Postgres is up - executing command"
exec echo "Postgres is up"