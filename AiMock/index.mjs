import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';

const port = 3033;
const mockPath = path.resolve('./response.json');

const server = http.createServer((_, res) => {
  const readStream = fs.createReadStream(mockPath);

  readStream.on('open', () => {
    readStream.pipe(res);
  });

  readStream.on('error', (err) => {
    console.error(err);
    res.end(err);
  });
});

server.listen(port, () =>
  console.log(`Server started at http://localhost:${port}`)
);
