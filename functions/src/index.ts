/* eslint-disable object-curly-spacing */
/* eslint-disable indent */
/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/no-var-requires */
/* eslint-disable quotes */
import * as functions from 'firebase-functions';
import * as admin from 'firebase-admin';
import express from 'express';
import cors from 'cors';

const pem: string = functions.config().unityscores.privatekey.replace(/\\n/g, '\n');

admin.initializeApp({
  credential: admin.credential.cert({
    clientEmail: functions.config().unityscores.clientemail,
    privateKey: pem,
    projectId: functions.config().unityscores.projectid,
  }),
});

const firestore = admin.firestore();

const app = express();

// Automatically allow cross-origin requests
app.use(cors({ origin: true }));

interface IScoreDocument {
  name: string,
  score: number,
  date: number,
}

const collection = firestore.collection('scores');

app.get('/applyScore', async (req, res) => {
  const name = req.query['name'] as string;
  const score = Number.parseInt(req.query['score'] as string);

  if (!Number.isFinite(score) || !name || name.length > 50) {
    res.sendStatus(400);
    return;
  }

  const document: IScoreDocument = {
    name,
    score,
    date: new Date().getTime(),
  };

  await collection.add(document);
  res.sendStatus(200);
});

exports.api = functions.https.onRequest(app);

