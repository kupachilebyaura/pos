forgotPassword(email: string) {
  return this.http.post('/api/auth/forgot-password', { email });
}
resetPassword(token: string, newPassword: string) {
  return this.http.post('/api/auth/reset-password', { token, newPassword });
}