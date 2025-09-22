import { getToken } from "./AuthService";
const API_URL = "http://localhost:51400/api";

export const getAllQuestions = async () => {
  const res = await fetch(`${API_URL}/questions/get-all`, {
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });

  if (!res.ok) return [];
  return await res.json();
};

export const createQuestion = async (data) => {
  const formData = new FormData();
  formData.append("title", data.title);
  formData.append("description", data.description);
  if (data.questionImage) {
    formData.append("questionImage", data.questionImage);
  }

  const res = await fetch(`${API_URL}/questions/create`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
    body: formData,
  });

  return res.ok;
};

export const getQuestionById = async (id) => {
  const res = await fetch(`${API_URL}/questions/${id}`, {
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });

  if (!res.ok) return null;
  return await res.json();
};

export const getMyQuestions = async () => {
  const res = await fetch(`${API_URL}/questions/my-questions`, {
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });
  if (!res.ok) return [];
  return await res.json();
};

export const deleteQuestion = async (id) => {
  const res = await fetch(`${API_URL}/questions/delete/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
  });
  return res.ok;
};

export const updateQuestion = async (id, data) => {
  const formData = new FormData();
  formData.append("title", data.title);
  formData.append("description", data.description);
  if (data.questionImage) {
    formData.append("questionImage", data.questionImage);
  }

  const res = await fetch(`${API_URL}/questions/edit/${id}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${getToken()}`,
    },
    body: formData,
  });

  if (!res.ok) {
    const errorData = await res.json();
    return { success: false, errors: errorData.errors || [] };
  }
  return { success: true };
};

export const closeQuestion = async (questionId, topAnswerId) => {
  const res = await fetch(
    `${API_URL}/questions/close/${questionId}?topAnswerId=${topAnswerId}`,
    {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${getToken()}`,
      },
    }
  );
  return res.ok;
};
