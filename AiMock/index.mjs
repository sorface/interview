import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';

const port = 3033;
const urlMocks = {
  '/ai-assistant/analyze': path.resolve('./responseAnalyze.json'),
  '/ai-assistant/examinee': path.resolve('./responseExaminee.json'),
};

const server = http.createServer((req, res) => {
  const readStream = fs.createReadStream(urlMocks[req.url]);

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
