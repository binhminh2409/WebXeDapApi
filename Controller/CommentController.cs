﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebXeDapAPI.Common;
using WebXeDapAPI.Dto;
using WebXeDapAPI.Service.Interfaces;

namespace WebXeDapAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CommentDto commentDto)
        {
            try
            {
                var result = await _commentService.Create(commentDto);
                return Ok(new XBaseResult
                {
                    data = result,
                    success = true,
                    httpStatusCode = (int)HttpStatusCode.OK,
                    message = "Create Commnent Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new XBaseResult
                {
                    success = false,
                    httpStatusCode = (int)HttpStatusCode.BadRequest,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int productId)
        {
            try
            {
                var result = await _commentService.GetCommentsByUserId(productId);
                return Ok(new XBaseResult
                {
                    data = result,
                    success = true,
                    httpStatusCode = (int)HttpStatusCode.OK,
                    message = "Create Commnent Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new XBaseResult
                {
                    success = false,
                    httpStatusCode = (int)HttpStatusCode.BadRequest,
                    message = ex.Message
                });
            }
        }


        [HttpGet("api/Comment/All")]
        public async Task<IActionResult> GetAll(int productId)
        {
            try
            {
                var result = await _commentService.GetAll(productId);
                return Ok(new XBaseResult
                {
                    data = result,
                    success = true,
                    httpStatusCode = (int)HttpStatusCode.OK,
                    message = "Create Commnent Successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new XBaseResult
                {
                    success = false,
                    httpStatusCode = (int)HttpStatusCode.BadRequest,
                    message = ex.Message
                });
            }
        }
    }
}
