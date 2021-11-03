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

app.get('/allTimeHigh', async (req, res) => {
  const scores = await collection
    .orderBy('score', 'desc')
    .limit(10)
    .get();
  res.send(JSON.stringify(scores.docs.map((x) => {
    const data = x.data() as IScoreDocument;
    const date = new Date();
    date.setUTCMilliseconds(data.date);
    return {
      ...data,
      date: date.toJSON(),
    };
  })));
});

app.get('/todaysHigh', async (req, res) => {
  const threshold = new Date();
  threshold.setHours(0);
  threshold.setMinutes(0);
  threshold.setSeconds(0);

  const scores = await collection
    .orderBy('date', 'desc')
    .where('date', '>=', threshold.getTime())
    .orderBy('score', 'desc')
    .limit(10)
    .get();

  res.send(JSON.stringify(scores.docs.map((x) => {
    const data = x.data() as IScoreDocument;
    const date = new Date(data.date);
    return {
      ...data,
      date: date.toJSON(),
    };
  })));
});

exports.api = functions.https.onRequest(app);
