import { getToken } from "./AuthService";

const API_URL = import.meta.env.VITE_BACKEND_API_URL;

export const createAnswer = async (answerData) => {
  const token = getToken();
  const response = await fetch(`${API_URL}/answers/create`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(answerData),
  });

  if (!response.ok) {
    throw new Error("Failed to create answer");
  }

  return await response.text(); // ili json() ako backend vrati objekt
};

export const getAnswersByQuestionId = async (questionId) => {
  const token = getToken();
  const response = await fetch(`${API_URL}/answers/by-question/${questionId}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to fetch answers");
  }

  return await response.json(); // očekujemo niz odgovora
};
