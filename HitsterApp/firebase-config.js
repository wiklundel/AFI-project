// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
import { getFirestore } from "firebase/firestore";

// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyDSAlmdpJ7vS_JgN6EPAulVILZxoKeZ-nA",
  authDomain: "hitsterapp-1902d.firebaseapp.com",
  projectId: "hitsterapp-1902d",
  storageBucket: "hitsterapp-1902d.firebasestorage.app",
  messagingSenderId: "31402365072",
  appId: "1:31402365072:web:4389847be2edcd9b510815",
  measurementId: "G-9YFH34150V",
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
