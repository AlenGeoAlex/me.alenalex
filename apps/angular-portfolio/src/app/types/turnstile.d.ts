declare const turnstile: {
  render: (
    container: string | HTMLElement,
    options: {
      sitekey: string;
      callback?: (token: string) => void;
      "error-callback"?: (errorCode: string) => void;
      theme?: 'light' | 'dark' | 'auto';
      size?: 'normal' | 'compact';
    }
  ) => string;
  reset?: (widgetId?: string) => void;
  getResponse: (widgetId: string) => string;
};
