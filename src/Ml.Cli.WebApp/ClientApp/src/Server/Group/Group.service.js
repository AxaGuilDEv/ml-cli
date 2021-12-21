export const fetchGroups = fetch => async () => fetch('groups');

export const fetchUsers = fetch => async () => fetch('users');

export const fetchDeleteGroup = fetch => async id =>
  fetch(`groups/${id}`, {
    method: 'DELETE',
  });

export const fetchGroup = fetch => async id =>
  fetch(`groups/${id}`, {
    method: 'GET',
  });

export const fetchCreateOrUpdateGroup = fetch => async newGroup =>
  fetch('groups', {
    method: 'POST',
    body: JSON.stringify(newGroup),
  });