getOwnProfile() {
  return this.http.get<User>('http://localhost:5000/api/profile');
}
updateOwnProfile(data: any) {
  return this.http.put('http://localhost:5000/api/profile', data);
}