import { getToken } from "./AuthService";

const API_URL = "http://localhost:51400/api";

export const voteForAnswer = async (answerId) => {
  const response = await fetch(`${API_URL}/vote/create/${answerId}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });

  return response.ok;
};

export const unvoteAnswer = async (answerId) => {
  const response = await fetch(`${API_URL}/vote/delete/${answerId}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });

  return response.ok;
};

export const hasUserVoted = async (answerId) => {
  const response = await fetch(`${API_URL}/vote/has-voted/${answerId}`, {
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });

  if (!response.ok) return false;
  return await response.json();
};
