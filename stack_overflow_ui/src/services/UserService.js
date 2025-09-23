const API_URL = "http://localhost:51400/api";

const createRegisterRequestForm = (formData) => {
  const form = new FormData();
  for (const key in formData) {
    if (formData[key] !== null && formData[key] !== "") {
      form.append(key, formData[key]);
    }
  }
  return form;
};

export const registerUser = async (formData) => {
  const form = createRegisterRequestForm(formData);
  return await fetch(`${API_URL}/users/create`, {
    method: "POST",
    body: form,
  });
};

export const loginUser = async (loginRequest) => {
  return await fetch(`${API_URL}/users/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(loginRequest),
  });
};

export const fetchUserProfilePicture = async (token) => {
  const response = await fetch(`${API_URL}/users/profilepicture`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const data = await response.json();

  if (!data.success || !data.profilePictureUrl) {
    throw new Error("Failed to fetch profile picture URL");
  }

  return data.profilePictureUrl;
};

export const fetchUserProfile = async (token) => {
  const response = await fetch(`${API_URL}/users/profile`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) throw new Error("Failed to fetch user profile");

  return await response.json();
};

export const updateUserProfile = async (formData, token) => {
  const response = await fetch(`${API_URL}/users/update`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData?.message || "Failed to update profile");
  }

  return await response.text();
};
