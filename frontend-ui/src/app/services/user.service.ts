getOwnProfile() {
  return this.http.get<User>('/api/profile');
}
updateOwnProfile(data: any) {
  return this.http.put('/api/profile', data);
}