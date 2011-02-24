﻿namespace tinyweb.framework
{
    public static class Result
    {
        public static HtmlResult Html(string filepath)
        {
            return new HtmlResult(filepath);
        }

        public static FileResult File(string filepath)
        {
            return new FileResult(filepath);
        }

        public static JsonResult Json(object data)
        {
            return new JsonResult(data);
        }

        public static StringResult String(string data)
        {
            return new StringResult(data);
        }

        public static RedirectResult<T> Redirect<T>(object arguments = null)
        {
            return new RedirectResult<T>(arguments);
        }

        public static RedirectResult Redirect(string uri)
        {
            return new RedirectResult(uri);
        }
    }
}