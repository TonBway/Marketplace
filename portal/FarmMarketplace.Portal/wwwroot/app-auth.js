window.fmAuth = {
  login: async function (request) {
    const response = await fetch('/portal-auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include',
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      return null;
    }

    return await response.json();
  },

  logout: async function () {
    await fetch('/portal-auth/logout', {
      method: 'POST',
      credentials: 'include'
    });
  }
};
