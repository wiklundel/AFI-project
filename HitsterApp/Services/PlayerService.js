import { collection, addDoc } from "firebase/firestore";
import { db } from "../firebase";

export async function addPlayerToGame(gameId, username) {
  const player = {
    username: username,
    score: 0,
    tokens: 0,
    timeline: [],
  };

  const docRef = await addDoc(
    collection(db, "games", gameId, "players"),
    player,
  );

  return docRef.id;
}
