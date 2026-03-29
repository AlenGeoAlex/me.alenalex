export * from './auth.service';
import { AuthService } from './auth.service';
export * from './post.service';
import { PostService } from './post.service';
export const APIS = [AuthService, PostService];
